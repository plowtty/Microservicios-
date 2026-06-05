namespace Auth.UnitTests.Application;

using Auth.Application.Commands.Login;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class LoginHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokensMock = new();

    private LoginCommandHandler CreateHandler() =>
        new(_usersMock.Object, _hasherMock.Object, _tokensMock.Object);

    private static User MakeUser(string email = "user@test.com") =>
        User.Create(email, "$2a$12$hashedpassword", "John", "Doe");

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnTokens()
    {
        var user = MakeUser();
        var expiry = DateTime.UtcNow.AddMinutes(15);
        var pair = new Auth.Application.DTOs.TokenPairDto(
            "access-token", "jti-123", expiry, "refresh-token", expiry.AddDays(7));

        _usersMock.Setup(u => u.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _tokensMock.Setup(t => t.GenerateTokenPair(user)).Returns(pair);
        _tokensMock.Setup(t => t.HashToken(It.IsAny<string>())).Returns("hashed");
        _tokensMock.Setup(t => t.GenerateRefreshTokenValue()).Returns("raw-token");

        var result = await CreateHandler().Handle(new LoginCommand("user@test.com", "Password1!"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        _usersMock.Verify(u => u.UpdateAsync(user, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnFailure()
    {
        var user = MakeUser();
        _usersMock.Setup(u => u.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await CreateHandler().Handle(new LoginCommand("user@test.com", "wrong"), default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Invalid");
        _usersMock.Verify(u => u.UpdateAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnFailure_WithoutTimingLeak()
    {
        // Even when user not found, BCrypt.Verify is still called (timing-safe)
        _usersMock.Setup(u => u.GetByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync((User?)null);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var result = await CreateHandler().Handle(new LoginCommand("unknown@test.com", "any"), default);

        result.IsSuccess.Should().BeFalse();
        // Verify BCrypt was still called despite user not existing (prevents timing attacks)
        _hasherMock.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnFailure()
    {
        var user = MakeUser();
        user.Deactivate();
        _usersMock.Setup(u => u.GetByEmailAsync("user@test.com", default)).ReturnsAsync(user);
        _hasherMock.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var result = await CreateHandler().Handle(new LoginCommand("user@test.com", "Password1!"), default);

        result.IsSuccess.Should().BeFalse();
    }
}
