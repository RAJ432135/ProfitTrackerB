using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("today")]
    public async Task<ActionResult<DashboardSummary>> Today(CancellationToken ct)
        => Ok(await _dashboardService.GetTodayAsync(ct));

    [HttpGet("month")]
    public async Task<ActionResult<DashboardSummary>> Month(CancellationToken ct)
        => Ok(await _dashboardService.GetMonthAsync(ct));
}
