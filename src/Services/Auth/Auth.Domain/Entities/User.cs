namespace Auth.Domain.Entities;

using Auth.Domain.Enums;
using Auth.Domain.Events;
using Auth.Domain.Exceptions;
using Shared.Common;

public class User : AggregateRoot
{
    private readonly List<RefreshToken> _refreshTokens = [];

    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public IReadOnlyList<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private User() { }

    public static User Create(string email, string passwordHash, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegisteredEvent(user.Id, user.Email, user.Role.ToString()));
        return user;
    }

    public void AddRefreshToken(RefreshToken token) => _refreshTokens.Add(token);

    public RefreshToken? GetActiveRefreshToken(string tokenHash)
        => _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow);

    public void RevokeRefreshToken(string tokenHash, string reason)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.TokenHash == tokenHash)
            ?? throw new InvalidCredentialsException("Refresh token not found");

        token.Revoke(reason);
    }

    public void RevokeAllRefreshTokens(string reason)
    {
        foreach (var token in _refreshTokens.Where(t => !t.IsRevoked))
            token.Revoke(reason);
    }

    public void RecordLogin() => LastLoginAt = DateTime.UtcNow;

    public void Deactivate() => IsActive = false;
}
