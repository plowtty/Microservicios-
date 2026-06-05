namespace Auth.Application.Commands.Login;

using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using MediatR;
using Shared.Common;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public LoginCommandHandler(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Result<AuthTokenDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _users.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        // Constant-time comparison: always verify hash even if user not found
        var dummyHash = "$2a$12$invalidhashpaddingtomakethisconst";
        var hashToVerify = user?.PasswordHash ?? dummyHash;
        var valid = _hasher.Verify(request.Password, hashToVerify);

        if (user is null || !valid || !user.IsActive)
            return Result<AuthTokenDto>.Failure("Invalid email or password.");

        var pair = _tokens.GenerateTokenPair(user);
        var tokenHash = _tokens.HashToken(pair.RefreshTokenValue);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, pair.Jti, pair.RefreshTokenExpiry);
        user.AddRefreshToken(refreshToken);
        user.RecordLogin();

        await _users.UpdateAsync(user, ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            pair.AccessToken,
            pair.RefreshTokenValue,
            pair.AccessTokenExpiry,
            pair.RefreshTokenExpiry));
    }
}
