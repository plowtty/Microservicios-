namespace Auth.Application.Commands.RevokeToken;

using MediatR;
using Shared.Common;

public record RevokeTokenCommand(string RefreshToken) : IRequest<Result<bool>>;
