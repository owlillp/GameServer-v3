namespace RealtimeService.Core.Configuration;

public sealed class RealtimeOptions
{
    public const string SECTION_NAME = "Realtime";

    public int TicketTtlSeconds { get; init; } = 60;
}
