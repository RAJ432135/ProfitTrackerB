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

    [HttpGet("week")]
    public async Task<ActionResult<DashboardSummary>> Week(CancellationToken ct)
        => Ok(await _dashboardService.GetWeekAsync(ct));

    [HttpGet("month")]
    public async Task<ActionResult<DashboardSummary>> Month(CancellationToken ct)
        => Ok(await _dashboardService.GetMonthAsync(ct));

    [HttpGet("last-month")]
    public async Task<ActionResult<DashboardSummary>> LastMonth(CancellationToken ct)
        => Ok(await _dashboardService.GetLastMonthAsync(ct));

    [HttpGet("year")]
    public async Task<ActionResult<DashboardSummary>> Year([FromQuery] int? year, CancellationToken ct)
        => Ok(await _dashboardService.GetYearAsync(year, ct));

    [HttpGet("range")]
    public async Task<ActionResult<DashboardSummary>> Range([FromQuery] DateTime fromDate, [FromQuery] DateTime toDate, CancellationToken ct)
        => Ok(await _dashboardService.GetRangeAsync(fromDate, toDate, ct));
}
