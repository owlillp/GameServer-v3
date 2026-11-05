using AuthService.Contracts.Admin;
using AuthService.Core.Features.Admin.Queries.GetStats;
using AuthService.Core.Features.Admin.Queries.ListUsers;
using AuthService.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authentication.Authorization;
using Shared.Core.Abstractions;
using Shared.Framework.Endpoints;
using Shared.SharedKernel.Responses;

namespace AuthService.Web.Endpoints.Admin;

public sealed class AdminEndpoints : IEndpointGroup
{
    private const string RESOURCE = "/admin";

    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RESOURCE)
            .WithTags("Admin")
            .RequireAuthorization();

        group.MapGet("/users", ListUsers)
            .WithName("Admin.Users.List")
            .WithSummary("Список пользователей с поиском и пагинацией")
            .RequirePermissions(AuthPermissions.Users.VIEW);

        group.MapGet("/stats", GetStats)
            .WithName("Admin.Stats.Get")
            .WithSummary("Сводная статистика аккаунтов")
            .RequirePermissions(AuthPermissions.Users.VIEW);

        return group;
    }

    private static async Task<EndpointResult<PaginationResponse<AdminUserDto>>> ListUsers(
        [FromQuery] int page,
        [FromQuery] int pageSize,
        [FromQuery] string? search,
        [FromServices] IQueryHandler<PaginationResponse<AdminUserDto>, ListUsersQuery> handler,
        CancellationToken ct) =>
        await handler.Handle(new ListUsersQuery(page, pageSize, search), ct);

    private static async Task<EndpointResult<AdminStatsDto>> GetStats(
        [FromServices] IQueryHandler<AdminStatsDto, GetStatsQuery> handler,
        CancellationToken ct) =>
        await handler.Handle(new GetStatsQuery(), ct);
}
