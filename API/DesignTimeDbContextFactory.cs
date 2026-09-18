using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using VehicleProfitTracker.API.Data;

namespace VehicleProfitTracker.API;

/// <summary>
/// Used only by `dotnet ef` design-time tooling (migrations add/update), never at
/// app runtime. Reads DefaultConnection from appsettings(.Development).json, or
/// from DB_CONNECTION / ConnectionStrings__DefaultConnection env vars if set.
/// </summary>
public class VehicleProfitTrackerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connection = Environment.GetEnvironmentVariable("DB_CONNECTION")
                          ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                          ?? configuration.GetConnectionString("DefaultConnection")
                          ?? throw new InvalidOperationException(
                              "No connection string found. Set DB_CONNECTION, " +
                              "ConnectionStrings__DefaultConnection, or DefaultConnection in appsettings.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connection);

        return new AppDbContext(optionsBuilder.Options);
    }
}
