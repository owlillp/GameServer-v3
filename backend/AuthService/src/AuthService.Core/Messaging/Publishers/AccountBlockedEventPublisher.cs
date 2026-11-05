using Shared.IntegrationEvents.Identity.Events;

namespace AuthService.Core.Messaging.Publishers;

public interface IAccountBlockedEventPublisher
{
    Task PublishAsync(Guid id);
}

public sealed class AccountBlockedEventPublisher(IOutboxService outbox) : IAccountBlockedEventPublisher
{
    public Task PublishAsync(Guid id) => outbox.PublishAsync(new AccountBlockedEvent(id));
}
