using Microsoft.AspNetCore.Mvc;
using ProfileService.Contracts.Dtos;
using ProfileService.Core.Features.Profiles.Queries.GetMyProfile;
using Shared.Core.Abstractions;
using Shared.Framework.Endpoints;

namespace ProfileService.Web.Endpoints.Profiles;

public sealed class ProfileEndpoints : IEndpointGroup
{
    private const string RESOURCE = "/profiles";

    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RESOURCE)
            .WithTags("Profiles")
            .RequireAuthorization();

        group.MapGet("/me", GetMyProfile)
            .WithName("Profiles.Me.Get")
            .WithSummary("Профиль текущего игрока");

        return group;
    }

    private static async Task<EndpointResult<ProfileDto>> GetMyProfile(
        [FromServices] IQueryHandler<ProfileDto, GetMyProfileQuery> handler,
        CancellationToken ct) =>
        await handler.Handle(new GetMyProfileQuery(), ct);
}
