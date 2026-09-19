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

    public Task<DashboardSummary> GetTodayAsync(CancellationToken ct = default)
    {
        var date = DateTime.UtcNow.Date;
        var period = date.ToString("MMMM dd, yyyy");
        return BuildSummaryAsync(date, date, period, ct);
    }

    public Task<DashboardSummary> GetMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var period = now.ToString("MMMM yyyy");
        return BuildSummaryAsync(start, end, period, ct);
    }

    public Task<DashboardSummary> GetLastMonthAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var lastMonth = now.AddMonths(-1);
        var start = new DateTime(lastMonth.Year, lastMonth.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        var period = lastMonth.ToString("MMMM yyyy");
        return BuildSummaryAsync(start, end, period, ct);
    }

    public Task<DashboardSummary> GetYearAsync(int? year = null, CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var start = new DateTime(targetYear, 1, 1);
        var end = new DateTime(targetYear, 12, 31);
        var period = targetYear.ToString();
        return BuildSummaryAsync(start, end, period, ct);
    }

    public Task<DashboardSummary> GetWeekAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow.Date;
        var start = now.AddDays(-(int)now.DayOfWeek);
        var end = start.AddDays(6);
        var period = $"{start:MMM dd} - {end:MMM dd, yyyy}";
        return BuildSummaryAsync(start, end, period, ct);
    }

    public Task<DashboardSummary> GetRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        var from = fromDate.Date;
        var to = toDate.Date;
        var period = $"{from:MMM dd, yyyy} - {to:MMM dd, yyyy}";
        return BuildSummaryAsync(from, to, period, ct);
    }

    private async Task<DashboardSummary> BuildSummaryAsync(DateTime from, DateTime to, string period, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");

        // Npgsql maps DateTime columns to 'timestamp with time zone' and rejects any
        // parameter whose Kind isn't Utc. `new DateTime(y, m, d)` and model-bound query
        // params both come back as Kind=Unspecified, which is what was causing the 500s
        // on every dashboard route except /today (which happened to use .Date on an
        // already-Utc value). Normalize both ends here so every caller is covered.
        from = DateTime.SpecifyKind(from.Date, DateTimeKind.Utc);
        to = DateTime.SpecifyKind(to.Date, DateTimeKind.Utc);

        var vehicles = await _db.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync(ct);

        // Extend 'to' to include entire day (next day at 00:00)
        var endOfDay = to.AddDays(1);

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

        return new DashboardSummary(period, from, to, totalIncome, totalExpense, totalIncome - totalExpense, vehicleSummaries);
    }
}