using EventBooking.Application.DTOs.Auth;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Auth;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Application.Services.Auth;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EventBooking.UnitTests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _sut;

    private static readonly DateTimeOffset Now = new(2026, 8, 27, 12, 0, 0, TimeSpan.Zero);

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);

        // Every successful path through AuthService (Register/Login/RefreshToken) calls
        // IssueTokens -> IJwtTokenGenerator.GenerateRefreshToken(). Give it a sane default
        // here so individual tests only need to override it when the returned value itself
        // is under test (e.g. asserting rotation linkage).
        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("default-refresh-token", Now.AddDays(7)));
    }

    // ==========================================
    // RegisterAsync
    // ==========================================

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsEmailAlreadyExistsException_AndDoesNotPersist()
    {
        var request = new RegisterRequest("Sara", "Ahmed", "Sara@Example.com", "P@ssw0rd1");
        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync("sara@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = async () => await _sut.RegisterAsync(request);

        await act.Should().ThrowAsync<EmailAlreadyExistsException>();
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmail_ToLowercaseAndTrimmed_BeforeCheckingExistence()
    {
        var request = new RegisterRequest("Sara", "Ahmed", "  Sara@Example.com  ", "P@ssw0rd1");
        _userRepositoryMock
            .Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("token");

        await _sut.RegisterAsync(request);

        _userRepositoryMock.Verify(r => r.ExistsByEmailAsync("sara@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_OnSuccess_AlwaysAssignsUserRole_RegardlessOfInput()
    {
        // Security requirement: RegisterRequest has no Role field at all, so there is no
        // way for a client to influence it — this locks in that the created User is
        // hard-coded to UserRole.User, never Admin.
        User? capturedUser = null;
        var request = new RegisterRequest("Omar", "Khaled", "omar@example.com", "P@ssw0rd1");
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);
        _passwordHasherMock.Setup(h => h.HashPassword(request.Password)).Returns("hashed-password");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");

        var response = await _sut.RegisterAsync(request);

        capturedUser.Should().NotBeNull();
        capturedUser!.Role.Should().Be(UserRole.User);
        capturedUser.PasswordHash.Should().Be("hashed-password");
        response.Role.Should().Be(UserRole.User.ToString());
        response.Token.Should().Be("jwt-token");
        response.RefreshToken.Should().Be("default-refresh-token");
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_OnSuccess_PersistsARefreshTokenLinkedToTheNewUser()
    {
        User? capturedUser = null;
        RefreshToken? capturedRefreshToken = null;
        var request = new RegisterRequest("Nour", "Adel", "nour@example.com", "P@ssw0rd1");
        _userRepositoryMock.Setup(r => r.ExistsByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => capturedUser = u)
            .Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedRefreshToken = rt)
            .Returns(Task.CompletedTask);
        _passwordHasherMock.Setup(h => h.HashPassword(It.IsAny<string>())).Returns("hashed");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt-token");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns(new RefreshTokenResult("rt-abc", Now.AddDays(7)));

        var response = await _sut.RegisterAsync(request);

        capturedRefreshToken.Should().NotBeNull();
        capturedRefreshToken!.UserId.Should().Be(capturedUser!.Id);
        capturedRefreshToken.Token.Should().Be("rt-abc");
        response.RefreshToken.Should().Be("rt-abc");
        response.RefreshTokenExpiresAt.Should().Be(Now.AddDays(7));
    }

    // ==========================================
    // LoginAsync
    // ==========================================

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsInvalidCredentialsException()
    {
        // Deliberately the exact same exception as "wrong password" below — this
        // protects against user-enumeration attacks, per the AuthService comment.
        var request = new LoginRequest("nouser@example.com", "whatever");
        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("nouser@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ThrowsInvalidCredentialsException()
    {
        var user = BuildUser();
        var request = new LoginRequest(user.Email, "WrongPassword1!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);

        var act = async () => await _sut.LoginAsync(request);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithAccessAndRefreshTokens()
    {
        var user = BuildUser();
        var request = new LoginRequest(user.Email, "CorrectPassword1!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(user)).Returns("jwt-token-123");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns(new RefreshTokenResult("rt-123", Now.AddDays(7)));

        var response = await _sut.LoginAsync(request);

        response.Token.Should().Be("jwt-token-123");
        response.RefreshToken.Should().Be("rt-123");
        response.RefreshTokenExpiresAt.Should().Be(Now.AddDays(7));
        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
        _refreshTokenRepositoryMock.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // RefreshTokenAsync
    // ==========================================

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenDoesNotExist_ThrowsInvalidRefreshTokenException()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("unknown-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _sut.RefreshTokenAsync("unknown-token");

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsExpiredButNotRevoked_ThrowsInvalidRefreshTokenException()
    {
        var expiredToken = BuildRefreshToken(expiresAt: Now.AddDays(-1));
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(expiredToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var act = async () => await _sut.RefreshTokenAsync(expiredToken.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsAlreadyRevoked_ThrowsInvalidRefreshTokenException_AndRevokesAllOtherActiveTokensForThatUser()
    {
        // Reuse-detection: presenting an already-rotated/revoked token is treated as a
        // possible theft signal. Every other still-active token belonging to the same
        // user must be revoked as a precaution, forcing re-login everywhere.
        var userId = Guid.NewGuid();
        var revokedToken = BuildRefreshToken(userId: userId, expiresAt: Now.AddDays(5));
        revokedToken.Revoke();

        var otherActiveToken1 = BuildRefreshToken(userId: userId, expiresAt: Now.AddDays(5));
        var otherActiveToken2 = BuildRefreshToken(userId: userId, expiresAt: Now.AddDays(5));

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(revokedToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);
        _refreshTokenRepositoryMock
            .Setup(r => r.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken> { otherActiveToken1, otherActiveToken2 });

        var act = async () => await _sut.RefreshTokenAsync(revokedToken.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        otherActiveToken1.IsRevoked.Should().BeTrue();
        otherActiveToken2.IsRevoked.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenRevokedAndNoOtherActiveTokensExist_ThrowsWithoutCallingSaveChanges()
    {
        var userId = Guid.NewGuid();
        var revokedToken = BuildRefreshToken(userId: userId, expiresAt: Now.AddDays(5));
        revokedToken.Revoke();

        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(revokedToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(revokedToken);
        _refreshTokenRepositoryMock
            .Setup(r => r.GetActiveTokensByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        var act = async () => await _sut.RefreshTokenAsync(revokedToken.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenUserBehindTokenNoLongerExists_ThrowsInvalidRefreshTokenException()
    {
        var validToken = BuildRefreshToken(expiresAt: Now.AddDays(5));
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(validToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validToken);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(validToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _sut.RefreshTokenAsync(validToken.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_OnSuccess_RevokesThePresentedToken_LinksItToTheReplacement_AndPersistsOnce()
    {
        var user = BuildUser();
        var presentedToken = BuildRefreshToken(userId: user.Id, expiresAt: Now.AddDays(5));
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(presentedToken.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(presentedToken);
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(user)).Returns("new-access-token");
        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns(new RefreshTokenResult("new-refresh-token", Now.AddDays(7)));

        RefreshToken? capturedNewToken = null;
        _refreshTokenRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Callback<RefreshToken, CancellationToken>((rt, _) => capturedNewToken = rt)
            .Returns(Task.CompletedTask);

        var response = await _sut.RefreshTokenAsync(presentedToken.Token);

        response.Token.Should().Be("new-access-token");
        response.RefreshToken.Should().Be("new-refresh-token");

        presentedToken.IsRevoked.Should().BeTrue();
        presentedToken.ReplacedByToken.Should().Be("new-refresh-token");

        capturedNewToken.Should().NotBeNull();
        capturedNewToken!.UserId.Should().Be(user.Id);
        capturedNewToken.Token.Should().Be("new-refresh-token");

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // RevokeTokenAsync
    // ==========================================

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenDoesNotExist_ThrowsInvalidRefreshTokenException()
    {
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var act = async () => await _sut.RevokeTokenAsync("missing");

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenIsAlreadyRevoked_ThrowsInvalidRefreshTokenException()
    {
        var token = BuildRefreshToken(expiresAt: Now.AddDays(5));
        token.Revoke();
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(token.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var act = async () => await _sut.RevokeTokenAsync(token.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenIsExpired_ThrowsInvalidRefreshTokenException()
    {
        var token = BuildRefreshToken(expiresAt: Now.AddDays(-1));
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(token.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        var act = async () => await _sut.RevokeTokenAsync(token.Token);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenIsActive_RevokesItAndPersistsOnce()
    {
        var token = BuildRefreshToken(expiresAt: Now.AddDays(5));
        _refreshTokenRepositoryMock
            .Setup(r => r.GetByTokenAsync(token.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        await _sut.RevokeTokenAsync(token.Token);

        token.IsRevoked.Should().BeTrue();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ==========================================
    // Test helpers
    // ==========================================

    private static User BuildUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hashed",
            Role = UserRole.User
        };
    }

    private static RefreshToken BuildRefreshToken(Guid? userId = null, DateTimeOffset? expiresAt = null)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            Token = Guid.NewGuid().ToString(),
            ExpiresAt = expiresAt ?? Now.AddDays(7)
        };
    }
}