using EventBooking.Domain.Entities;

namespace EventBooking.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    RefreshTokenResult GenerateRefreshToken();
}