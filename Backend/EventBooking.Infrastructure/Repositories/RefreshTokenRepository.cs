using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;
using EventBooking.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EventBooking.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        // Tracking is preserved intentionally: AuthService mutates the returned entity
        // (Revoke) and relies on change tracking before IUnitOfWork.SaveChangesAsync(),
        // matching the pattern used by the other repositories' GetByIdAsync methods.
        return await _context.RefreshTokens
            .SingleOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        // SaveChanges is intentionally not called here; commit responsibility remains
        // with IUnitOfWork.SaveChangesAsync(), consistent with the other repositories.
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
    }
}