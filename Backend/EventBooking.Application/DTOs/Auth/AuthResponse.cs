namespace EventBooking.Application.DTOs.Auth;

public record AuthResponse(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Token,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);