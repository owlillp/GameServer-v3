using System.Text.Json;

namespace Shared.IntegrationEvents.Realtime;

public sealed record RealtimeNotification(
    Guid Id,
    string Type,
    string Audience,
    DateTime OccurredAt,
    int Version,
    JsonElement Payload);

public static class RealtimeAudiences
{
    public const string LIVE_OPS = "liveops";

    public static string Player(Guid accountId) => $"player:{accountId}";
}
