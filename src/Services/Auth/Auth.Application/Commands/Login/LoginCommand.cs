namespace Auth.Application.Commands.Login;

using Auth.Application.DTOs;
using MediatR;
using Shared.Common;

public record LoginCommand(string Email, string Password) : IRequest<Result<AuthTokenDto>>;
