using Microsoft.AspNetCore.Mvc;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

/// <summary>
/// Read-only, unauthenticated feature-flag check. Deliberately not behind
/// [Authorize] — the login screen needs to know whether to show "Forgot
/// password?" before anyone is signed in. Changing the flags themselves is
/// admin-only and lives on AdminController.
/// </summary>
[ApiController]
[Route("api/v1/settings")]
public class SettingsController : ControllerBase
{
    private readonly AppSettingsService _settingsService;

    public SettingsController(AppSettingsService settingsService) => _settingsService = settingsService;

    [HttpGet]
    public async Task<ActionResult<AppSettingsResponse>> Get(CancellationToken ct)
    {
        var settings = await _settingsService.GetAsync(ct);
        return Ok(new AppSettingsResponse(settings.PasswordResetEnabled, settings.SubscriptionsEnabled));
    }
}