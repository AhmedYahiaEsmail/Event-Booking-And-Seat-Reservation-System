namespace EventBooking.Application.Interfaces.Auth;

// Return contract for IJwtTokenGenerator.GenerateRefreshToken(): the raw token value
// plus the expiry the generator computed from JwtSettings.RefreshTokenExpiryInDays.
// AuthService uses this to build the RefreshToken entity without knowing anything
// about JWT/token-generation configuration itself.
public record RefreshTokenResult(string Token, DateTimeOffset ExpiresAt);