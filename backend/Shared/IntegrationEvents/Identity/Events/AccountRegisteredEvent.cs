namespace Shared.IntegrationEvents.Identity.Events;

public record AccountRegisteredEvent(Guid AccountId, string Username);
