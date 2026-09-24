using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;

namespace VehicleProfitTracker.API.Services;

/// <summary>
/// Reads/writes the single app_settings row that backs the admin portal's
/// feature toggles (password reset, subscriptions, ...).
/// </summary>
public class AppSettingsService
{
    private readonly AppDbContext _db;

    public AppSettingsService(AppDbContext db) => _db = db;

    public async Task<Models.AppSettings> GetAsync(CancellationToken ct = default)
    {
        var settings = await _db.AppSettings.FirstOrDefaultAsync(ct);
        if (settings is null)
        {
            settings = new Models.AppSettings { Id = Models.AppSettings.WellKnownId };
            _db.AppSettings.Add(settings);
            await _db.SaveChangesAsync(ct);
        }
        return settings;
    }

    public async Task<Models.AppSettings> UpdateAsync(UpdateAppSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await GetAsync(ct);
        settings.PasswordResetEnabled = request.PasswordResetEnabled;
        settings.SubscriptionsEnabled = request.SubscriptionsEnabled;
        await _db.SaveChangesAsync(ct);
        return settings;
    }
}