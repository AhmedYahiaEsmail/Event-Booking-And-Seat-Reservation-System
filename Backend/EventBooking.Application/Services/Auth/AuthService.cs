using EventBooking.Application.DTOs.Auth;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Auth;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;

namespace EventBooking.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await _userRepository.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            throw new EmailAlreadyExistsException(normalizedEmail);
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Security requirement: Default role is strictly UserRole.User. Client selection is impossible.
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Role = UserRole.User
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var (response, refreshTokenEntity) = IssueTokens(user);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);

        // Single SaveChangesAsync: the new User row and its first RefreshToken row
        // commit together in one transaction, same pattern as ReservationService.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        // Generic failure handling to protect email enumeration attacks
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            throw new InvalidCredentialsException();
        }

        var (response, refreshTokenEntity) = IssueTokens(user);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (existingToken is null)
        {
            throw new InvalidRefreshTokenException();
        }

        if (existingToken.IsRevoked)
        {
            // Reuse of an already-rotated/revoked token is a signal of possible theft
            // (a legitimate client only ever presents the latest token it holds).
            // As a precaution, every other currently-active token for this user gets
            // revoked too, forcing re-login everywhere.
            var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(existingToken.UserId, cancellationToken);
            foreach (var token in activeTokens)
            {
                token.Revoke();
            }

            if (activeTokens.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            throw new InvalidRefreshTokenException();
        }

        if (existingToken.IsExpired)
        {
            throw new InvalidRefreshTokenException();
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidRefreshTokenException();
        }

        var (response, newRefreshTokenEntity) = IssueTokens(user);

        // Rotation: the presented token is revoked and linked to the token that
        // replaced it, so a later reuse attempt hits the IsRevoked branch above.
        existingToken.Revoke(newRefreshTokenEntity.Token);

        await _refreshTokenRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

        // Single SaveChangesAsync: old token's revocation and the new token's insert
        // commit together in one transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            throw new InvalidRefreshTokenException();
        }

        existingToken.Revoke();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private (AuthResponse Response, RefreshToken Entity) IssueTokens(User user)
    {
        var accessToken = _jwtTokenGenerator.GenerateToken(user);
        var refreshTokenResult = _jwtTokenGenerator.GenerateRefreshToken();

        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshTokenResult.Token,
            ExpiresAt = refreshTokenResult.ExpiresAt
        };

        var response = new AuthResponse(
            UserId: user.Id,
            FirstName: user.FirstName,
            LastName: user.LastName,
            Email: user.Email,
            Role: user.Role.ToString(),
            Token: accessToken,
            RefreshToken: refreshTokenResult.Token,
            RefreshTokenExpiresAt: refreshTokenResult.ExpiresAt);

        return (response, refreshTokenEntity);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
