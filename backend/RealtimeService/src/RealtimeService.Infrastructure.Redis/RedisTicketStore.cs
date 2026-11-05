using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using RealtimeService.Core.Abstractions;
using RealtimeService.Core.Configuration;
using StackExchange.Redis;

namespace RealtimeService.Infrastructure.Redis;

public sealed class RedisTicketStore(
    IConnectionMultiplexer redis,
    IOptions<RealtimeOptions> options) : ITicketStore
{
    private const string KEY_PREFIX = "realtime:ticket:";

    public async Task<string> IssueAsync(Guid accountId, CancellationToken cancellationToken)
    {
        string ticket = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

        TimeSpan ttl = TimeSpan.FromSeconds(Math.Max(5, options.Value.TicketTtlSeconds));

        await redis.GetDatabase().StringSetAsync(
            KEY_PREFIX + ticket,
            accountId.ToString("D"),
            ttl);

        return ticket;
    }

    public async Task<Guid?> ConsumeAsync(string ticket, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(ticket))
        {
            return null;
        }

        RedisValue value = await redis.GetDatabase().StringGetDeleteAsync(
            KEY_PREFIX + ticket.Trim());

        if (value.IsNull)
        {
            return null;
        }

        return Guid.TryParse(value.ToString(), out Guid accountId) && accountId != Guid.Empty
            ? accountId
            : null;
    }
}
