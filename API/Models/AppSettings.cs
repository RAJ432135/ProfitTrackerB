namespace VehicleProfitTracker.API.Models;

/// <summary>
/// Single-row table holding app-wide feature toggles controlled from the admin
/// portal. There is always exactly one row, at Id = WellKnownId, seeded by the
/// migration that creates this table.
/// </summary>
public class AppSettings : BaseEntity
{
    public static readonly Guid WellKnownId = Guid.Parse("00000000-0000-0000-0000-000000000001");

    public bool PasswordResetEnabled { get; set; } = true;
    public bool SubscriptionsEnabled { get; set; } = true;
}