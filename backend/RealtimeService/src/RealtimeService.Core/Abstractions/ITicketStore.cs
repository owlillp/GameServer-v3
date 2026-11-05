namespace RealtimeService.Core.Abstractions;

public interface ITicketStore
{
    Task<string> IssueAsync(Guid accountId, CancellationToken cancellationToken);

    Task<Guid?> ConsumeAsync(string ticket, CancellationToken cancellationToken);
}
