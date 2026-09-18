namespace VehicleProfitTracker.API.Services;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "VehicleProfitTracker";
    public string Audience { get; set; } = "VehicleProfitTrackerClient";
    public int AccessTokenExpiryMinutes { get; set; } = 60;
}
