namespace VehicleProfitTracker.API.Models;

/// <summary>Product analytics only. Never put a user's transaction values or notes here.</summary>
public class UserEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
