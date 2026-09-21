namespace VehicleProfitTracker.API.Models;

public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;   // login identifier, unique
    public string PasswordHash { get; set; } = string.Empty;

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public DateTime? LastLoginAt { get; set; }
    public UserRole Role { get; set; } = UserRole.User;

    public List<Vehicle> Vehicles { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
    public List<UserEvent> Events { get; set; } = new();
    public List<Subscription> Subscriptions { get; set; } = new();
}
