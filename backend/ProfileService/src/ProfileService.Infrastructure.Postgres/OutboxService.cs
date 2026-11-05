using ProfileService.Core.Messaging;
using Wolverine.EntityFrameworkCore;

namespace ProfileService.Infrastructure.Postgres;

public class OutboxService(IDbContextOutbox<ProfileServiceDbContext> outbox) : IOutboxService
{
    public async Task PublishAsync<T>(T message)
        where T : class => await outbox.PublishAsync(message);

    public Task FlushAsync() => outbox.FlushOutgoingMessagesAsync();
}