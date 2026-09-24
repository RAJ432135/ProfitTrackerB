using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Models;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly AppSettingsService _settingsService;

    public AdminController(AppDbContext db, AppSettingsService settingsService)
    {
        _db = db;
        _settingsService = settingsService;
    }

    [HttpGet("settings")]
    public async Task<ActionResult<AppSettingsResponse>> GetSettings(CancellationToken ct)
    {
        var settings = await _settingsService.GetAsync(ct);
        return Ok(new AppSettingsResponse(settings.PasswordResetEnabled, settings.SubscriptionsEnabled));
    }

    [HttpPut("settings")]
    public async Task<ActionResult<AppSettingsResponse>> UpdateSettings(UpdateAppSettingsRequest request, CancellationToken ct)
    {
        var settings = await _settingsService.UpdateAsync(request, ct);
        return Ok(new AppSettingsResponse(settings.PasswordResetEnabled, settings.SubscriptionsEnabled));
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<AdminMetricsResponse>> Metrics(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var week = today.AddDays(-6);
        var month = today.AddDays(-29);
        var regularSince = today.AddDays(-6);

        var activeSubscriptionQuery = _db.Subscriptions.Where(s =>
            s.Status == SubscriptionStatus.Active &&
            (s.ExpiresAt == null || s.ExpiresAt > now));

        var response = new AdminMetricsResponse(
            await _db.Users.CountAsync(u => u.CreatedAt >= today, ct),
            await _db.UserEvents.Where(e => e.OccurredAt >= today).Select(e => e.UserId).Distinct().CountAsync(ct),
            await _db.UserEvents.Where(e => e.OccurredAt >= week).Select(e => e.UserId).Distinct().CountAsync(ct),
            await _db.UserEvents.Where(e => e.OccurredAt >= month).Select(e => e.UserId).Distinct().CountAsync(ct),
            await _db.UserEvents.Where(e => e.OccurredAt >= regularSince)
                .GroupBy(e => e.UserId)
                .CountAsync(g => g.Select(e => e.OccurredAt.Date).Distinct().Count() >= 3, ct),
            await _db.Users.CountAsync(ct),
            await activeSubscriptionQuery.CountAsync(ct),
            0m); // Revenue remains zero until verified store receipts populate subscriptions.

        return Ok(response);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserResponse>>> Users([FromQuery] string? search, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 100);
        var users = _db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            users = users.Where(u => EF.Functions.ILike(u.Name, $"%{term}%") || EF.Functions.ILike(u.Phone, $"%{term}%"));
        }

        var result = await users.OrderByDescending(u => u.CreatedAt).Take(take)
            .Select(u => new AdminUserResponse(
                u.Id, u.Name, u.Phone, u.Role.ToString(), u.CreatedAt, u.LastLoginAt,
                _db.UserEvents.Where(e => e.UserId == u.Id).Max(e => (DateTime?)e.OccurredAt),
                _db.Subscriptions.Where(s => s.UserId == u.Id).OrderByDescending(s => s.UpdatedAt)
                    .Select(s => s.Status.ToString()).FirstOrDefault() ?? "None"))
            .ToListAsync(ct);
        return Ok(result);
    }
}

