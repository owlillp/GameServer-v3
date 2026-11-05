using Shared.IntegrationEvents.Realtime;

namespace RealtimeService.Core.Messaging;

public interface IRealtimePublisher
{
    Task PublishAsync(RealtimeNotification notification, CancellationToken cancellationToken);
}
