using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.DTOs;

public record VehicleCreateRequest(string VehicleNumber, VehicleType VehicleType);
public record VehicleUpdateRequest(string VehicleNumber, VehicleType VehicleType);

public record VehicleResponse(Guid Id, string VehicleNumber, VehicleType VehicleType, DateTime CreatedAt);
