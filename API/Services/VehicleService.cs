using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Exceptions;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Services;

public class VehicleService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public VehicleService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    private Guid RequireUserId() =>
        _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");

    public async Task<List<VehicleResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var userId = RequireUserId();

        return await _db.Vehicles
            .Where(v => v.UserId == userId)
            .OrderByDescending(v => v.CreatedAt)
            .Select(v => new VehicleResponse(v.Id, v.VehicleNumber, v.VehicleType, v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<VehicleResponse> CreateAsync(VehicleCreateRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleNumber))
            throw new ValidationAppException("vehicleNumber", "Vehicle number is required.");

        var userId = RequireUserId();

        var vehicle = new Vehicle
        {
            UserId = userId,
            VehicleNumber = request.VehicleNumber.Trim().ToUpperInvariant(),
            VehicleType = request.VehicleType
        };

        _db.Vehicles.Add(vehicle);
        await _db.SaveChangesAsync(ct);

        return new VehicleResponse(vehicle.Id, vehicle.VehicleNumber, vehicle.VehicleType, vehicle.CreatedAt);
    }

    public async Task<VehicleResponse> UpdateAsync(Guid id, VehicleUpdateRequest request, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId, ct)
            ?? throw new NotFoundException("Vehicle not found.");

        vehicle.VehicleNumber = request.VehicleNumber.Trim().ToUpperInvariant();
        vehicle.VehicleType = request.VehicleType;
        await _db.SaveChangesAsync(ct);

        return new VehicleResponse(vehicle.Id, vehicle.VehicleNumber, vehicle.VehicleType, vehicle.CreatedAt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var userId = RequireUserId();
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId, ct)
            ?? throw new NotFoundException("Vehicle not found.");

        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync(ct);
    }
}
