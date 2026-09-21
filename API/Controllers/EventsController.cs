using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Exceptions;
using VehicleProfitTracker.API.Models;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

[ApiController]
[Route("api/v1/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private static readonly HashSet<string> AllowedEvents = new(StringComparer.Ordinal)
    {
        "app_open", "vehicle_created", "transaction_created", "report_viewed"
    };

    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public EventsController(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpPost]
    public async Task<IActionResult> Track(TrackEventRequest request, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");
        if (!AllowedEvents.Contains(request.Name))
            throw new ValidationAppException("name", "Unsupported analytics event.");

        _db.UserEvents.Add(new UserEvent
        {
            UserId = userId,
            Name = request.Name,
            Platform = request.Platform?.Trim()[..Math.Min(request.Platform.Trim().Length, 30)],
            AppVersion = request.AppVersion?.Trim()[..Math.Min(request.AppVersion.Trim().Length, 30)]
        });
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
}
