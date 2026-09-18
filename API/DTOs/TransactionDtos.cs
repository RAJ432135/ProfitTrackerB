using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.DTOs;

public record TransactionCreateRequest(
    Guid VehicleId,
    TransactionType Type,
    TransactionCategory Category,
    decimal Amount,
    DateTime Date,
    string? Note
);

public record TransactionUpdateRequest(
    TransactionType Type,
    TransactionCategory Category,
    decimal Amount,
    DateTime Date,
    string? Note
);

public record TransactionResponse(
    Guid Id,
    Guid VehicleId,
    string VehicleNumber,
    TransactionType Type,
    TransactionCategory Category,
    decimal Amount,
    DateTime Date,
    string? Note
);
