using Shared.IntegrationEvents.Identity.Events;

namespace AuthService.Core.Messaging.Publishers;

public interface IAccountRegisteredEventPublisher
{
    Task PublishAsync(Guid id, string username);
}

public sealed class AccountRegisteredEventPublisher(IOutboxService outbox) : IAccountRegisteredEventPublisher
{
    public Task PublishAsync(Guid id, string username) => outbox.PublishAsync(new AccountRegisteredEvent(id, username));
}
