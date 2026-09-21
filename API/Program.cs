using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.Middleware;
using VehicleProfitTracker.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Real secrets (DB password, JWT signing key, etc.) go in this file, which is
// git-ignored — see appsettings.Local.json.example. It's loaded last so it
// overrides the placeholder values checked into appsettings.json / appsettings.Development.json.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// ---------- Services ----------

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Vehicle Profit Tracker API", Version = "v1" });

    var jwtScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// EF Core - PostgreSQL. Support both Railway (DATABASE_URL) and local/Supabase connections.
// Railway automatically provides DATABASE_URL environment variable.
var connectionString = builder.Configuration["DATABASE_URL"]
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Neither DATABASE_URL environment variable nor 'DefaultConnection' connection string is configured.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VehicleService>();
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<DashboardService>();

// CORS for the mobile app. Tighten origins for production.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? new[] { "http://localhost:19006" };
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT auth
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
        ClockSkew = TimeSpan.FromMinutes(2)
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});

// Basic rate limiting — protects auth endpoints from brute force.
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// ---------- Middleware pipeline ----------

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseIpRateLimiting();

// Railway (and most PaaS hosts) terminate TLS at their edge proxy — the
// container itself only ever receives plain HTTP. UseHttpsRedirection() would
// otherwise redirect every request, causing a loop in production.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Baseline security headers — cheap, broadly-applicable defense-in-depth.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), camera=(), microphone=()";
    await next();
});

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (!canConnect)
            return Results.Json(new { status = "unhealthy", db = "unreachable" }, statusCode: 503);

        return Results.Ok(new { status = "ok", db = "connected", timestampUtc = DateTime.UtcNow, build = "admin-analytics-subscriptions-2026-09-20" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { status = "unhealthy", db = "error", detail = ex.Message }, statusCode: 503);
    }
});

// Apply EF Core migrations automatically on startup. Wrapped in try/catch so the
// app can still start (and Swagger still loads) even if the database is
// temporarily unreachable — makes local debugging much easier.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database migration failed. Ensure the connection string is correct and the DB is reachable. Continuing without applying migrations.");
    }
}

app.Run();

// Needed so WebApplicationFactory<Program> works for integration tests.
public partial class Program { }
