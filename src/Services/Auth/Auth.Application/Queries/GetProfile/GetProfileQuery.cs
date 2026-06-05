namespace Auth.Application.Queries.GetProfile;

using Auth.Application.DTOs;
using MediatR;
using Shared.Common;

public record GetProfileQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;
