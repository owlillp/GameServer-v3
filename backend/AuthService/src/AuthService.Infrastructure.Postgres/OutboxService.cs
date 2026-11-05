using AuthService.Core.Messaging;
using Wolverine.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres;

public class OutboxService(IDbContextOutbox<AuthServiceDbContext> outbox) : IOutboxService
{
    public async Task PublishAsync<T>(T message)
        where T : class => await outbox.PublishAsync(message);

    public Task FlushAsync() => outbox.FlushOutgoingMessagesAsync();
}
