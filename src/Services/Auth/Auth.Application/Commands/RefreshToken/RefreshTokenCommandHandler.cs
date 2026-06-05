namespace Auth.Application.Commands.RefreshToken;

using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Interfaces;
using MediatR;
using Shared.Common;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public RefreshTokenCommandHandler(IUserRepository users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var tokenHash = _tokens.HashToken(request.RefreshToken);

        // We need to find the user by refresh token hash
        // This requires a join through UserRepository or a dedicated lookup
        var user = await _users.GetByRefreshTokenHashAsync(tokenHash, ct);
        if (user is null)
            return Result<AuthTokenDto>.Failure("Invalid refresh token.");

        var existingToken = user.GetActiveRefreshToken(tokenHash);
        if (existingToken is null)
            return Result<AuthTokenDto>.Failure("Refresh token is expired or revoked.");

        // Rotate: revoke old token, issue new pair
        user.RevokeRefreshToken(tokenHash, "Rotated");

        var pair = _tokens.GenerateTokenPair(user);
        var newHash = _tokens.HashToken(pair.RefreshTokenValue);
        var newRefreshToken = RefreshToken.Create(user.Id, newHash, pair.Jti, pair.RefreshTokenExpiry);
        user.AddRefreshToken(newRefreshToken);

        await _users.UpdateAsync(user, ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            pair.AccessToken,
            pair.RefreshTokenValue,
            pair.AccessTokenExpiry,
            pair.RefreshTokenExpiry));
    }
}
