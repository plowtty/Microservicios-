namespace Auth.UnitTests.Domain;

using Auth.Domain.Entities;
using Auth.Domain.Enums;
using Auth.Domain.Exceptions;
using FluentAssertions;
using Xunit;

public class UserTests
{
    private static User CreateUser(string email = "test@example.com") =>
        User.Create(email, "hashedpassword", "John", "Doe");

    [Fact]
    public void Create_ShouldSetDefaultCustomerRole()
    {
        var user = CreateUser();
        user.Role.Should().Be(UserRole.Customer);
        user.IsActive.Should().BeTrue();
        user.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Create_ShouldNormalizeEmailToLowercase()
    {
        var user = User.Create("Test@EXAMPLE.COM", "hash", "Jane", "Doe");
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_ShouldAllowAdminRole()
    {
        var user = User.Create("admin@test.com", "hash", "Admin", "User", UserRole.Admin);
        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void RecordLogin_ShouldSetLastLoginAt()
    {
        var user = CreateUser();
        user.LastLoginAt.Should().BeNull();

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddRefreshToken_ShouldStoreToken()
    {
        var user = CreateUser();
        var token = RefreshToken.Create(user.Id, "hash123", "jti-1", DateTime.UtcNow.AddDays(7));

        user.AddRefreshToken(token);

        user.RefreshTokens.Should().HaveCount(1);
    }

    [Fact]
    public void GetActiveRefreshToken_ShouldReturnToken_WhenValid()
    {
        var user = CreateUser();
        var token = RefreshToken.Create(user.Id, "hash123", "jti-1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(token);

        var found = user.GetActiveRefreshToken("hash123");

        found.Should().NotBeNull();
        found!.TokenHash.Should().Be("hash123");
    }

    [Fact]
    public void GetActiveRefreshToken_ShouldReturnNull_WhenRevoked()
    {
        var user = CreateUser();
        var token = RefreshToken.Create(user.Id, "hash123", "jti-1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(token);
        token.Revoke("test");

        var found = user.GetActiveRefreshToken("hash123");

        found.Should().BeNull();
    }

    [Fact]
    public void RevokeRefreshToken_ShouldMarkAsRevoked()
    {
        var user = CreateUser();
        var token = RefreshToken.Create(user.Id, "hash123", "jti-1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(token);

        user.RevokeRefreshToken("hash123", "Logout");

        token.IsRevoked.Should().BeTrue();
        token.RevokedReason.Should().Be("Logout");
    }

    [Fact]
    public void RevokeRefreshToken_WhenTokenNotFound_ShouldThrow()
    {
        var user = CreateUser();
        var act = () => user.RevokeRefreshToken("nonexistent", "reason");
        act.Should().Throw<InvalidCredentialsException>();
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAll()
    {
        var user = CreateUser();
        user.AddRefreshToken(RefreshToken.Create(user.Id, "h1", "j1", DateTime.UtcNow.AddDays(7)));
        user.AddRefreshToken(RefreshToken.Create(user.Id, "h2", "j2", DateTime.UtcNow.AddDays(7)));

        user.RevokeAllRefreshTokens("Security breach");

        user.RefreshTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        var user = CreateUser();
        user.Deactivate();
        user.IsActive.Should().BeFalse();
    }
}
