using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using ProfileService.Contracts.Dtos;
using ProfileService.Core.Abstractions;
using Shared.Authentication.Authentication.UserScope;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace ProfileService.Core.Features.Profiles.Queries.GetMyProfile;

public sealed class GetMyProfileHandler(
    IReadDbContext dbContext,
    UserScopedData userScope) : IQueryHandler<ProfileDto, GetMyProfileQuery>
{
    public async Task<Result<ProfileDto, Error>> Handle(
        GetMyProfileQuery query,
        CancellationToken cancellationToken = new ())
    {
        var userId = userScope.Id;
        if (userId is null)
        {
            return Error.Authentication("authentication.required", "User identity is not available");
        }

        var profile = await dbContext.ProfilesRead
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == userId.Value, cancellationToken);

        if (profile is null)
        {
            return GeneralErrors.NotFound(userId.Value, entityName: "profile");
        }

        return new ProfileDto(
            profile.Id,
            profile.Nickname.Value,
            profile.Region?.Value,
            userScope.Email,
            userScope.UserName,
            profile.CreatedAt,
            profile.UpdatedAt);
    }
}
