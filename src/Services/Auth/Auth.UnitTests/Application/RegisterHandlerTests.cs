namespace Auth.UnitTests.Application;

using Auth.Application.Commands.Register;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

public class RegisterHandlerTests
{
    private readonly Mock<IUserRepository> _usersMock = new();
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokensMock = new();

    private RegisterCommandHandler CreateHandler() =>
        new(_usersMock.Object, _hasherMock.Object, _tokensMock.Object);

    [Fact]
    public async Task Handle_NewEmail_ShouldRegisterAndReturnTokens()
    {
        var expiry = DateTime.UtcNow.AddMinutes(15);
        var pair = new TokenPairDto("access", "jti-1", expiry, "refresh", expiry.AddDays(7));

        _usersMock.Setup(u => u.ExistsByEmailAsync("new@test.com", default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("Password1!")).Returns("bcrypt-hash");
        _tokensMock.Setup(t => t.GenerateTokenPair(It.IsAny<User>())).Returns(pair);
        _tokensMock.Setup(t => t.HashToken(It.IsAny<string>())).Returns("token-hash");

        var command = new RegisterCommand("new@test.com", "Password1!", "Jane", "Doe");
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access");
        _usersMock.Verify(u => u.AddAsync(
            It.Is<User>(u => u.Email == "new@test.com"), default), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        _usersMock.Setup(u => u.ExistsByEmailAsync("dup@test.com", default)).ReturnsAsync(true);

        var command = new RegisterCommand("dup@test.com", "Password1!", "A", "B");
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already registered");
        _usersMock.Verify(u => u.AddAsync(It.IsAny<User>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword_NotStoreRaw()
    {
        var expiry = DateTime.UtcNow.AddMinutes(15);
        var pair = new TokenPairDto("t", "j", expiry, "r", expiry.AddDays(7));

        _usersMock.Setup(u => u.ExistsByEmailAsync(It.IsAny<string>(), default)).ReturnsAsync(false);
        _hasherMock.Setup(h => h.Hash("RawPass1!")).Returns("$2a$12$bcrypthash");
        _tokensMock.Setup(t => t.GenerateTokenPair(It.IsAny<User>())).Returns(pair);
        _tokensMock.Setup(t => t.HashToken(It.IsAny<string>())).Returns("h");

        var command = new RegisterCommand("a@b.com", "RawPass1!", "A", "B");
        await CreateHandler().Handle(command, default);

        _usersMock.Verify(u => u.AddAsync(
            It.Is<User>(u => u.PasswordHash == "$2a$12$bcrypthash" && u.PasswordHash != "RawPass1!"),
            default), Times.Once);
    }
}
