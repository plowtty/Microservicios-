namespace Auth.Application.Commands.Register;

using Auth.Application.DTOs;
using MediatR;
using Shared.Common;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<AuthTokenDto>>;
