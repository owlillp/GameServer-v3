using Microsoft.AspNetCore.SignalR;
using RealtimeService.Core.Messaging;
using RealtimeService.Web.Hubs;
using Shared.IntegrationEvents.Realtime;

namespace RealtimeService.Web.Services;

public sealed class SignalRRealtimePublisher(IHubContext<RealtimeHub> hubContext) : IRealtimePublisher
{
    public Task PublishAsync(
        RealtimeNotification notification,
        CancellationToken cancellationToken) =>
        hubContext.Clients
            .Group(notification.Audience)
            .SendAsync("Notify", notification, cancellationToken);
}
