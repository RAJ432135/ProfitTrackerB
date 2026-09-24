using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using VehicleProfitTracker.API.Data;
using VehicleProfitTracker.API.DTOs;
using VehicleProfitTracker.API.Exceptions;
using VehicleProfitTracker.API.Models;

namespace VehicleProfitTracker.API.Services;

/// <summary>
/// Handles authentication use cases: register, login (access+refresh token pair),
/// refresh (with rotation and reuse detection), logout, password reset.
/// </summary>
public class AuthService
{
    private const int RefreshTokenDaysDefault = 7;
    private const int RefreshTokenDaysRememberMe = 30;

    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext db,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ILogger<AuthService> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationAppException("name", "Name is required.");

        if (string.IsNullOrWhiteSpace(request.Phone))
            throw new ValidationAppException("phone", "Phone number is required.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
            throw new ValidationAppException("password", "Password must be at least 6 characters.");

        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, ct);
        if (existing is not null)
            throw new ConflictException("An account with this phone number already exists.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return new RegisterResponse(user.Id, user.Name, user.Phone);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, ct);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAppException("Invalid phone number or password.");

        user.LastLoginAt = DateTime.UtcNow;

        var (accessToken, refreshToken) = await IssueTokenPairAsync(user, request.RememberMe, ct);
        await _db.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, refreshToken, new UserSummary(user.Id, user.Name, user.Phone, user.Role.ToString()));
    }

    public async Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request, CancellationToken ct = default)
    {
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct)
            ?? throw new UnauthorizedAppException("Invalid refresh token.");

        if (!existing.IsActive)
        {
            // The token is inactive because it's either expired or already used
            // (rotated). A *rotated* token being presented again means someone
            // has a copy of a refresh token that's already been exchanged —
            // a classic sign of token theft. Treat it as a security incident:
            // revoke every other active refresh token for this user so a
            // stolen token can't keep minting new sessions.
            if (existing.RevokedAt is not null && existing.ReplacedByToken is not null)
            {
                var otherActiveTokens = await _db.RefreshTokens
                    .Where(t => t.UserId == existing.UserId && t.RevokedAt == null)
                    .ToListAsync(ct);

                foreach (var token in otherActiveTokens)
                    token.RevokedAt = DateTime.UtcNow;

                if (otherActiveTokens.Count > 0)
                    await _db.SaveChangesAsync(ct);

                _logger.LogWarning(
                    "Refresh token reuse detected for user {UserId}. Revoked {Count} active session(s).",
                    existing.UserId, otherActiveTokens.Count);

                throw new UnauthorizedAppException("This session is no longer valid. Please log in again.");
            }

            throw new UnauthorizedAppException("Refresh token expired or already used.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == existing.UserId, ct)
            ?? throw new UnauthorizedAppException("Invalid refresh token.");

        var wasLongLived = (existing.ExpiresAt - existing.CreatedAt).TotalDays > RefreshTokenDaysDefault;
        var (accessToken, newRefreshToken) = await IssueTokenPairAsync(user, wasLongLived, ct);

        existing.RevokedAt = DateTime.UtcNow;
        existing.ReplacedByToken = newRefreshToken;

        await _db.SaveChangesAsync(ct);

        return new RefreshTokenResponse(accessToken, newRefreshToken);
    }

    public async Task LogoutAsync(LogoutRequest request, CancellationToken ct = default)
    {
        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (existing is null) return;

        existing.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, ct);
        if (user is null) return; // don't reveal whether the phone exists

        var resetToken = GenerateResetCode();
        user.PasswordResetToken = resetToken;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(15);
        await _db.SaveChangesAsync(ct);

        // In production this goes out over SMS. For MVP, log it so it can be
        // read from server logs during the pilot phase — no SMS provider needed yet.
        _logger.LogInformation("Password reset code for {Phone}: {Code}", user.Phone, resetToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            throw new ValidationAppException("newPassword", "Password must be at least 6 characters.");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Phone == request.Phone, ct)
            ?? throw new NotFoundException("User not found.");

        if (user.PasswordResetToken != request.Token ||
            user.PasswordResetTokenExpiresAt is null ||
            user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
        {
            throw new ValidationAppException("token", "Invalid or expired reset code.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiresAt = null;

        // A password reset should sign out any existing session too.
        var activeTokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }

    private static string GenerateResetCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private async Task<(string AccessToken, string RefreshToken)> IssueTokenPairAsync(
        User user, bool rememberMe, CancellationToken ct)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshTokenValue = _jwtService.GenerateRefreshTokenValue();

        var days = rememberMe ? RefreshTokenDaysRememberMe : RefreshTokenDaysDefault;
        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(days)
        });

        await Task.CompletedTask;
        return (accessToken, refreshTokenValue);
    }
}
