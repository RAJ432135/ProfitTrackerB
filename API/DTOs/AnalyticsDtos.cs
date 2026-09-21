namespace VehicleProfitTracker.API.DTOs;

public record TrackEventRequest(string Name, string? Platform, string? AppVersion);

public record AdminMetricsResponse(
    int NewRegistrations,
    int DailyActiveUsers,
    int WeeklyActiveUsers,
    int MonthlyActiveUsers,
    int RegularUsers,
    int TotalUsers,
    int ActiveSubscriptions,
    decimal MonthlyRecurringRevenue);

public record AdminUserResponse(
    Guid Id,
    string Name,
    string Phone,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    DateTime? LastSeenAt,
    string SubscriptionStatus);
