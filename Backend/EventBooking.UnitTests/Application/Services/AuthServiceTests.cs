using EventBooking.Application.DTOs.Auth;
using EventBooking.Application.Exceptions;
using EventBooking.Application.Interfaces.Auth;
using EventBooking.Application.Interfaces.Persistence;
using EventBooking.Application.Services.Auth;
using EventBooking.Domain.Entities;
using EventBooking.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventBooking.UnitTests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object);
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
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
    public async Task LoginAsync_WithValidCredentials_ReturnsAuthResponseWithToken()
    {
        var user = BuildUser();
        var request = new LoginRequest(user.Email, "CorrectPassword1!");
        _userRepositoryMock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasherMock.Setup(h => h.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateToken(user)).Returns("jwt-token-123");

        var response = await _sut.LoginAsync(request);

        response.Token.Should().Be("jwt-token-123");
        response.UserId.Should().Be(user.Id);
        response.Email.Should().Be(user.Email);
    }

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
}