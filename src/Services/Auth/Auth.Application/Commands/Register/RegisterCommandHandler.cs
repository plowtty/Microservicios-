namespace Auth.Application.Commands.Register;

using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Interfaces;
using MediatR;
using Shared.Common;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthTokenDto>>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;

    public RegisterCommandHandler(IUserRepository users, IPasswordHasher hasher, ITokenService tokens)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
    }

    public async Task<Result<AuthTokenDto>> Handle(RegisterCommand request, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(request.Email, ct))
            return Result<AuthTokenDto>.Failure($"Email '{request.Email}' is already registered.");

        var passwordHash = _hasher.Hash(request.Password);
        var user = User.Create(request.Email, passwordHash, request.FirstName, request.LastName);

        var pair = _tokens.GenerateTokenPair(user);
        var tokenHash = _tokens.HashToken(pair.RefreshTokenValue);
        var refreshToken = RefreshToken.Create(user.Id, tokenHash, pair.Jti, pair.RefreshTokenExpiry);
        user.AddRefreshToken(refreshToken);

        await _users.AddAsync(user, ct);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            pair.AccessToken,
            pair.RefreshTokenValue,
            pair.AccessTokenExpiry,
            pair.RefreshTokenExpiry));
    }
}
