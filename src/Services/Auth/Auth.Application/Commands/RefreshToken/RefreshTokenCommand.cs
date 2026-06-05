namespace Auth.Application.Commands.RefreshToken;

using Auth.Application.DTOs;
using MediatR;
using Shared.Common;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<AuthTokenDto>>;
