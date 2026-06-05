namespace Auth.Application.Queries.GetProfile;

using Auth.Application.DTOs;
using Auth.Domain.Interfaces;
using MediatR;
using Shared.Common;

public class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, Result<UserProfileDto>>
{
    private readonly IUserRepository _users;

    public GetProfileQueryHandler(IUserRepository users) => _users = users;

    public async Task<Result<UserProfileDto>> Handle(GetProfileQuery request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result<UserProfileDto>.Failure("User not found.");

        return Result<UserProfileDto>.Success(new UserProfileDto(
            user.Id, user.Email, user.FirstName, user.LastName,
            user.Role.ToString(), user.CreatedAt, user.LastLoginAt));
    }
}
