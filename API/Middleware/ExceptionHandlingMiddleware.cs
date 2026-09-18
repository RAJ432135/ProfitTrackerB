using System.Net;
using System.Text.Json;
using VehicleProfitTracker.API.Exceptions;

namespace VehicleProfitTracker.API.Middleware;

/// <summary>
/// Global exception handling + consistent API error format.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            ValidationAppException vex => (HttpStatusCode.BadRequest, vex.Message, (object?)vex.Errors),
            NotFoundException nfe => (HttpStatusCode.NotFound, nfe.Message, null),
            ConflictException ce => (HttpStatusCode.Conflict, ce.Message, null),
            UnauthorizedAppException uae => (HttpStatusCode.Unauthorized, uae.Message, null),
            ForbiddenAppException fae => (HttpStatusCode.Forbidden, fae.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning(exception, "Handled exception: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new Dictionary<string, object?>
        {
            ["status"] = (int)statusCode,
            ["message"] = message,
            ["errors"] = errors,
            ["traceId"] = context.TraceIdentifier
        };

        if (statusCode == HttpStatusCode.InternalServerError && _env.IsDevelopment())
        {
            payload["exception"] = exception.ToString();
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
