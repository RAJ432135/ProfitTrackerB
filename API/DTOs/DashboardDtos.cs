namespace VehicleProfitTracker.API.DTOs;

public record VehicleSummary(Guid VehicleId, string VehicleNumber, decimal Income, decimal Expense, decimal Profit);

public record DashboardSummary(
    string Period,
    DateTime StartDate,
    DateTime EndDate,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal TotalProfit,
    List<VehicleSummary> Vehicles
);
