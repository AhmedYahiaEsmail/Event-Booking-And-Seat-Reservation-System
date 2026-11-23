using EventBooking.Domain.Entities;

namespace EventBooking.Application.Interfaces.Persistence;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    // Used for reuse-detection on refresh: if an already-rotated/revoked token is
    // presented again, every other still-active token for that user is revoked as a
    // precaution (see AuthService.RefreshTokenAsync).
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}