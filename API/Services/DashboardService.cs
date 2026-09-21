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

    // Vehicle Profit Tracker is India-only today. Until per-user timezones are
    // supported, every "today" / "this month" / etc. period is anchored to IST
    // rather than UTC. Without this, a trip logged after 5:30 PM IST (which is
    // already midnight UTC) gets silently counted under the wrong calendar day
    // for the user — e.g. a 1 AM IST entry would show up under "yesterday".
    private static readonly TimeSpan IstOffset = TimeSpan.FromHours(5.5);

    private static DateTime NowIst() => DateTime.UtcNow + IstOffset;

    // Converts an IST calendar-day boundary (a plain DateTime at 00:00, understood
    // to mean "midnight in IST") into the equivalent precise UTC instant that
    // Postgres/Npgsql actually needs to compare against.
    private static DateTime IstMidnightToUtcInstant(DateTime istCalendarDay) =>
        DateTime.SpecifyKind(istCalendarDay.Date - IstOffset, DateTimeKind.Utc);

    public Task<DashboardSummary> GetTodayAsync(CancellationToken ct = default)
    {
        var todayIst = NowIst().Date;
        var period = todayIst.ToString("MMMM dd, yyyy");
        var fromUtc = IstMidnightToUtcInstant(todayIst);
        var toUtcExclusive = IstMidnightToUtcInstant(todayIst.AddDays(1));
        return BuildSummaryAsync(fromUtc, toUtcExclusive, period, ct);
    }

    public Task<DashboardSummary> GetWeekAsync(CancellationToken ct = default)
    {
        var todayIst = NowIst().Date;
        var startIst = todayIst.AddDays(-(int)todayIst.DayOfWeek);
        var endIstExclusive = startIst.AddDays(7);
        var period = $"{startIst:MMM dd} - {startIst.AddDays(6):MMM dd, yyyy}";
        return BuildSummaryAsync(IstMidnightToUtcInstant(startIst), IstMidnightToUtcInstant(endIstExclusive), period, ct);
    }

    public Task<DashboardSummary> GetMonthAsync(CancellationToken ct = default)
    {
        var nowIst = NowIst();
        var startIst = new DateTime(nowIst.Year, nowIst.Month, 1);
        var endIstExclusive = startIst.AddMonths(1);
        var period = nowIst.ToString("MMMM yyyy");
        return BuildSummaryAsync(IstMidnightToUtcInstant(startIst), IstMidnightToUtcInstant(endIstExclusive), period, ct);
    }

    public Task<DashboardSummary> GetLastMonthAsync(CancellationToken ct = default)
    {
        var lastMonthIst = NowIst().AddMonths(-1);
        var startIst = new DateTime(lastMonthIst.Year, lastMonthIst.Month, 1);
        var endIstExclusive = startIst.AddMonths(1);
        var period = lastMonthIst.ToString("MMMM yyyy");
        return BuildSummaryAsync(IstMidnightToUtcInstant(startIst), IstMidnightToUtcInstant(endIstExclusive), period, ct);
    }

    public Task<DashboardSummary> GetYearAsync(int? year = null, CancellationToken ct = default)
    {
        var targetYear = year ?? NowIst().Year;
        var startIst = new DateTime(targetYear, 1, 1);
        var endIstExclusive = new DateTime(targetYear + 1, 1, 1);
        var period = targetYear.ToString();
        return BuildSummaryAsync(IstMidnightToUtcInstant(startIst), IstMidnightToUtcInstant(endIstExclusive), period, ct);
    }

    public Task<DashboardSummary> GetRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken ct = default)
    {
        // fromDate/toDate come from the client's date picker as plain calendar
        // dates. Treat them as IST calendar days, consistent with every other
        // period above, rather than as UTC instants.
        var startIst = fromDate.Date;
        var endInclusiveIst = toDate.Date;
        var endIstExclusive = endInclusiveIst.AddDays(1);
        var period = $"{startIst:MMM dd, yyyy} - {endInclusiveIst:MMM dd, yyyy}";
        return BuildSummaryAsync(IstMidnightToUtcInstant(startIst), IstMidnightToUtcInstant(endIstExclusive), period, ct);
    }

    private async Task<DashboardSummary> BuildSummaryAsync(DateTime fromUtc, DateTime toUtcExclusive, string period, CancellationToken ct)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedAppException("Not authenticated.");

        // fromUtc/toUtcExclusive are already exact UTC instants (IST day
        // boundaries converted to UTC). Only normalize Kind for Npgsql here —
        // do NOT call .Date on these, that would silently discard the IST
        // offset and reintroduce the original UTC-day-boundary bug.
        fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);
        toUtcExclusive = DateTime.SpecifyKind(toUtcExclusive, DateTimeKind.Utc);

        var vehicles = await _db.Vehicles
            .Where(v => v.UserId == userId)
            .ToListAsync(ct);

        var transactions = await _db.Transactions
            .Where(t => t.Vehicle!.UserId == userId && t.Date >= fromUtc && t.Date < toUtcExclusive)
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

        return new DashboardSummary(period, fromUtc, toUtcExclusive.AddTicks(-1), totalIncome, totalExpense, totalIncome - totalExpense, vehicleSummaries);
    }
}
