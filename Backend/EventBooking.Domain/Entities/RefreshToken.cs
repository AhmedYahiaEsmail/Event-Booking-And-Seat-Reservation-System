using EventBooking.Domain.Exceptions;

namespace EventBooking.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
    public required string Token { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; private set; }

    // Set when this token is rotated out in favor of a newer one (see
    // AuthService.RefreshTokenAsync). Used purely for audit/reuse-detection; the new
    // token itself doesn't reference this one back.
    public string? ReplacedByToken { get; private set; }

    // Navigation property (no reciprocal collection needed on User).
    public User? User { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsActive => !IsRevoked && !IsExpired;

    public void Revoke(string? replacedByToken = null)
    {
        if (IsRevoked)
            throw new DomainException("Refresh token is already revoked.");

        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByToken = replacedByToken;
    }
}