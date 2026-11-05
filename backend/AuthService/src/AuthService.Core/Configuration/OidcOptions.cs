namespace AuthService.Core.Configuration;

public sealed class OidcOptions
{
    public const string SECTION_NAME = "OpenIddict";

    public string? Issuer { get; init; }

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 30;

    public OidcClientsOptions Clients { get; init; } = new();
}

public sealed class OidcClientsOptions
{
    public OidcClientOptions Web { get; init; } = new();

    public OidcClientOptions Unity { get; init; } = new();

    public OidcClientOptions ClanService { get; init; } = new();

    public OidcClientOptions WalletService { get; init; } = new();

    public OidcClientOptions CatalogService { get; init; } = new();

    public OidcClientOptions InventoryService { get; init; } = new();

    public OidcClientOptions StoreService { get; init; } = new();

    public OidcClientOptions EventsService { get; init; } = new();
}

public sealed class OidcClientOptions
{
    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public IReadOnlyList<string> RedirectUris { get; init; } = [];

    public IReadOnlyList<string> PostLogoutRedirectUris { get; init; } = [];

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ClientId);

    public bool IsPublic => string.IsNullOrWhiteSpace(ClientSecret);
}