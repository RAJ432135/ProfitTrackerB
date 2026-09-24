namespace VehicleProfitTracker.API.DTOs;

public record RegisterRequest(string Name, string Phone, string Password);
public record LoginRequest(string Phone, string Password, bool RememberMe = false);
public record RefreshTokenRequest(string RefreshToken);
public record LogoutRequest(string RefreshToken);

public record ForgotPasswordRequest(string Phone);
public record ResetPasswordRequest(string Phone, string Token, string NewPassword);

public record UserSummary(Guid Id, string Name, string Phone, string Role);

public record LoginResponse(string AccessToken, string RefreshToken, UserSummary User);
public record RefreshTokenResponse(string AccessToken, string RefreshToken);
public record RegisterResponse(Guid Id, string Name, string Phone);
