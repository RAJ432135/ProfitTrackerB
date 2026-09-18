using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

[ApiController]
[Route("api/v1/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly VehicleService _vehicleService;

    public VehiclesController(VehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VehicleResponse>>> GetAll(CancellationToken ct)
        => Ok(await _vehicleService.GetAllAsync(ct));

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create(VehicleCreateRequest request, CancellationToken ct)
        => Ok(await _vehicleService.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> Update(Guid id, VehicleUpdateRequest request, CancellationToken ct)
        => Ok(await _vehicleService.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _vehicleService.DeleteAsync(id, ct);
        return NoContent();
    }
}
