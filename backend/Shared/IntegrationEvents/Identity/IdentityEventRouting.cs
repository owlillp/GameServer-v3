namespace Shared.IntegrationEvents.Identity;

public static class IdentityEventRouting
{
    public const string EXCHANGE = "identity-events";

    public static class RoutingKeys
    {
        public const string ACCOUNT_REGISTERED = "registered.account";
        public const string ACCOUNT_DELETED = "deleted.account";
        public const string ACCOUNT_BLOCKED = "blocked.account";

        public const string ALL_ACCOUNT_EVENTS = "*.account";
    }
}
