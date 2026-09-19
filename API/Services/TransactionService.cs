using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Exceptions;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Services;

public class TransactionService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public TransactionService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");

    public async Task<List<TransactionResponse>> GetAllAsync(
        Guid? vehicleId, DateTime? from, DateTime? to, TransactionCategory? category, CancellationToken ct = default)
    {
        var userId = RequireUserId();

        var query = _db.Transactions
            .Include(t => t.Vehicle)
            .Where(t => t.Vehicle!.UserId == userId);

        if (vehicleId.HasValue) query = query.Where(t => t.VehicleId == vehicleId.Value);
        // Npgsql requires Kind=Utc for 'timestamp with time zone' columns; query-string
        // bound DateTimes come back as Kind=Unspecified and throw at the DB otherwise.
        if (from.HasValue) query = query.Where(t => t.Date >= DateTime.SpecifyKind(from.Value.Date, DateTimeKind.Utc));
        if (to.HasValue) query = query.Where(t => t.Date <= DateTime.SpecifyKind(to.Value.Date, DateTimeKind.Utc));
        if (category.HasValue) query = query.Where(t => t.Category == category.Value);

        return await query
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionResponse(
                t.Id, t.VehicleId, t.Vehicle!.VehicleNumber, t.Type, t.Category, t.Amount, t.Date, t.Note))
            .ToListAsync(ct);
    }

    public async Task<TransactionResponse> CreateAsync(TransactionCreateRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new ValidationAppException("amount", "Amount must be greater than zero.");

        var userId = RequireUserId();

        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == request.VehicleId && v.UserId == userId, ct)
            ?? throw new ValidationAppException("vehicleId", "Vehicle not found or does not belong to you.");

        var txn = new Transaction
        {
            VehicleId = request.VehicleId,
            Type = request.Type,
            Category = request.Category,
            Amount = request.Amount,
            Date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc),
            Note = request.Note
        };

        _db.Transactions.Add(txn);
        await _db.SaveChangesAsync(ct);

        return new TransactionResponse(txn.Id, txn.VehicleId, vehicle.VehicleNumber, txn.Type, txn.Category, txn.Amount, txn.Date, txn.Note);
    }

    public async Task<TransactionResponse> UpdateAsync(Guid id, TransactionUpdateRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new ValidationAppException("amount", "Amount must be greater than zero.");

        var userId = RequireUserId();

        var txn = await _db.Transactions
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == id && t.Vehicle!.UserId == userId, ct)
            ?? throw new NotFoundException("Transaction not found.");

        txn.Type = request.Type;
        txn.Category = request.Category;
        txn.Amount = request.Amount;
        txn.Date = DateTime.SpecifyKind(request.Date.Date, DateTimeKind.Utc);
        txn.Note = request.Note;

        await _db.SaveChangesAsync(ct);

        return new TransactionResponse(txn.Id, txn.VehicleId, txn.Vehicle!.VehicleNumber, txn.Type, txn.Category, txn.Amount, txn.Date, txn.Note);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = RequireUserId();

        var txn = await _db.Transactions
            .Include(t => t.Vehicle)
            .FirstOrDefaultAsync(t => t.Id == id && t.Vehicle!.UserId == userId, ct)
            ?? throw new NotFoundException("Transaction not found.");

        _db.Transactions.Remove(txn);
        await _db.SaveChangesAsync(ct);
    }
}