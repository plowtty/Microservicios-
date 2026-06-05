namespace Auth.Application.Commands.RevokeToken;

using Auth.Application.Interfaces;
using Auth.Domain.Interfaces;
using MediatR;
using Shared.Common;

public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result<bool>>
{
    private readonly IUserRepository _users;
    private readonly ITokenService _tokens;

    public RevokeTokenCommandHandler(IUserRepository users, ITokenService tokens)
    {
        _users = users;
        _tokens = tokens;
    }

    public async Task<Result<bool>> Handle(RevokeTokenCommand request, CancellationToken ct)
    {
        var tokenHash = _tokens.HashToken(request.RefreshToken);
        var user = await _users.GetByRefreshTokenHashAsync(tokenHash, ct);
        if (user is null)
            return Result<bool>.Failure("Token not found.");

        user.RevokeRefreshToken(tokenHash, "Logout");
        await _users.UpdateAsync(user, ct);

        return Result<bool>.Success(true);
    }
}
