namespace VehicleProfitTracker.API.Models;

/// <summary>
/// The server owns entitlement state. Payment-store webhooks will populate this
/// table in a later billing integration; the client must never mark itself paid.
/// </summary>
public class Subscription : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Store { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.None;
    public DateTime? StartsAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool AutoRenewEnabled { get; set; }
    public string? ExternalPurchaseId { get; set; }
}
