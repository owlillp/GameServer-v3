using Microsoft.AspNetCore.Mvc;
using RealtimeService.Contracts.Dtos;
using RealtimeService.Core.Features.Realtime.Commands.IssueTicket;
using Shared.Authentication.Authentication.UserScope;
using Shared.Core.Abstractions;
using Shared.Framework.Endpoints;

namespace RealtimeService.Web.Endpoints.Realtime;

public sealed class RealtimeEndpoints : IEndpointGroup
{
    private const string RESOURCE = "/realtime";

    public RouteGroupBuilder MapGroup(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app
            .MapGroup(RESOURCE)
            .WithTags("Realtime")
            .RequireAuthorization();

        group.MapPost("/ticket", IssueTicket)
            .WithName("Realtime.Ticket")
            .WithSummary("Issue a one-time ticket for the realtime WebSocket connection");

        return group;
    }

    private static async Task<EndpointResult<RealtimeTicketDto>> IssueTicket(
        [FromServices] ICommandHandler<RealtimeTicketDto, IssueTicketCommand> handler,
        [FromServices] UserScopedData userScope,
        CancellationToken ct) =>
        await handler.Handle(new IssueTicketCommand(userScope.Id ?? Guid.Empty), ct);
}
