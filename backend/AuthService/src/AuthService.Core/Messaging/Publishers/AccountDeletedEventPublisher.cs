using Shared.IntegrationEvents.Identity.Events;

namespace AuthService.Core.Messaging.Publishers;

public interface IAccountDeletedEventPublisher
{
    Task PublishAsync(Guid id);
}

public sealed class AccountDeletedEventPublisher(IOutboxService outbox) : IAccountDeletedEventPublisher
{
    public Task PublishAsync(Guid id) => outbox.PublishAsync(new AccountDeletedEvent(id));
}
