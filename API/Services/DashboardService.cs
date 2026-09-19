using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Exceptions;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Services;

public class DashboardService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DashboardService(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public Task<DashboardSummary> GetTodayAsync(CancellationToken ct = default) =>
        BuildSummaryAsync(DateTime.UtcNow.Date, DateTime.UtcNow.Date, ct);

    public Task<DashboardSummary> GetMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return BuildSummaryAsync(start, end, ct);
    }

    private async Task<DashboardSummary> BuildSummaryAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");

        var vehicles = await _db.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync(ct);

        // Extend 'to' to include entire day (next day at 00:00)
        var endOfDay = to.AddDays(1).Date;

        var transactions = await _db.Transactions
            .Where(t => t.Vehicle!.UserId == userId && t.Date >= from && t.Date < endOfDay)
            .ToListAsync(ct);

        // Profit is always Income - Expense, computed here in C# — never trusted
        // from the client and never delegated to an AI call.
        var vehicleSummaries = vehicles.Select(v =>
        {
            var vTxns = transactions.Where(t => t.VehicleId == v.Id);
            var income = vTxns.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = vTxns.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
            return new VehicleSummary(v.Id, v.VehicleNumber, income, expense, income - expense);
        }).ToList();

        var totalIncome = vehicleSummaries.Sum(v => v.Income);
        var totalExpense = vehicleSummaries.Sum(v => v.Expense);

        return new DashboardSummary(totalIncome, totalExpense, totalIncome - totalExpense, vehicleSummaries);
    }
}
