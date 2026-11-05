namespace AuthService.Core.Configuration;

public static class OidcScopes
{
    public const string OPEN_ID = "openid";
    public const string PROFILE = "profile";
    public const string EMAIL = "email";
    public const string OFFLINE_ACCESS = "offline_access";

    public const string AUTH = "auth";
    public const string CLANS = "clans";
    public const string PROFILES = "profiles";
    public const string WALLET = "wallet";
    public const string CATALOG = "catalog";
    public const string INVENTORY = "inventory";
    public const string STORE = "store";
    public const string EVENTS = "events";
    public const string REALTIME = "realtime";

    public static IReadOnlyList<string> All { get; } =
    [
        OPEN_ID,
        PROFILE,
        EMAIL,
        OFFLINE_ACCESS,
        AUTH,
        CLANS,
        PROFILES,
        WALLET,
        CATALOG,
        INVENTORY,
        STORE,
        EVENTS,
        REALTIME,
    ];

    public static class Resources
    {
        public const string AUTH_RESOURCE = "auth-service";
        public const string CLANS_RESOURCE = "clan-service";
        public const string PROFILES_RESOURCE = "profile-service";
        public const string WALLET_RESOURCE = "wallet-service";
        public const string CATALOG_RESOURCE = "catalog-service";
        public const string INVENTORY_RESOURCE = "inventory-service";
        public const string STORE_RESOURCE = "store-service";
        public const string EVENT_RESOURCE = "event-service";
        public const string REALTIME_RESOURCE = "realtime-service";
    }
}