using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Models;
using VehicleProfitTracker.API.Services;

namespace VehicleProfitTracker.API.Controllers;

[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly TransactionService _transactionService;

    public TransactionsController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    // GET /api/v1/transactions?vehicleId=&from=&to=&category=
    [HttpGet]
    public async Task<ActionResult<List<TransactionResponse>>> GetAll(
        [FromQuery] Guid? vehicleId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] TransactionCategory? category,
        CancellationToken ct)
        => Ok(await _transactionService.GetAllAsync(vehicleId, from, to, category, ct));

    [HttpPost]
    public async Task<ActionResult<TransactionResponse>> Create(TransactionCreateRequest request, CancellationToken ct)
        => Ok(await _transactionService.CreateAsync(request, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TransactionResponse>> Update(Guid id, TransactionUpdateRequest request, CancellationToken ct)
        => Ok(await _transactionService.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _transactionService.DeleteAsync(id, ct);
        return NoContent();
    }
}
