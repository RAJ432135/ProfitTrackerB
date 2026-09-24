namespace VehicleProfitTracker.API.DTOs;

public record AppSettingsResponse(bool PasswordResetEnabled, bool SubscriptionsEnabled);

public record UpdateAppSettingsRequest(bool PasswordResetEnabled, bool SubscriptionsEnabled);