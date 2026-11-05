using AuthService.Core.Configuration;
using AuthService.Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;

namespace AuthService.Core.Services;

public sealed class PlatformSyncService(
    RoleManager<Role> roleManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager)
{
    public async Task SyncAsync(OidcOptions options, CancellationToken cancellationToken)
    {
        await SeedRolesAsync();

        await UpsertScopeAsync(OidcScopes.AUTH, "AuthService API Access", OidcScopes.Resources.AUTH_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.CLANS, "ClanService API Access", OidcScopes.Resources.CLANS_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.PROFILES, "ProfileService API Access", OidcScopes.Resources.PROFILES_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.WALLET, "WalletService API Access", OidcScopes.Resources.WALLET_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.CATALOG, "CatalogService API Access", OidcScopes.Resources.CATALOG_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.INVENTORY, "InventoryService API Access", OidcScopes.Resources.INVENTORY_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.STORE, "StoreService API Access", OidcScopes.Resources.STORE_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.EVENTS, "EventService API Access", OidcScopes.Resources.EVENT_RESOURCE, cancellationToken);
        await UpsertScopeAsync(OidcScopes.REALTIME, "RealtimeService API Access", OidcScopes.Resources.REALTIME_RESOURCE, cancellationToken);

        await UpsertPublicClientAsync(options.Clients.Web, allowPasswordGrant: false, cancellationToken);
        await UpsertPublicClientAsync(options.Clients.Unity, allowPasswordGrant: true, cancellationToken);
        await UpsertServiceClientAsync(options.Clients.ClanService, [OidcScopes.AUTH], cancellationToken);
        await UpsertServiceClientAsync(options.Clients.WalletService, [OidcScopes.WALLET], cancellationToken);
        await UpsertServiceClientAsync(options.Clients.CatalogService, [OidcScopes.CATALOG], cancellationToken);
        await UpsertServiceClientAsync(options.Clients.InventoryService, [OidcScopes.INVENTORY], cancellationToken);
        await UpsertServiceClientAsync(
            options.Clients.StoreService,
            [OidcScopes.STORE, OidcScopes.WALLET, OidcScopes.CATALOG, OidcScopes.INVENTORY],
            cancellationToken);
        await UpsertServiceClientAsync(options.Clients.EventsService, [OidcScopes.EVENTS], cancellationToken);
    }

    private async Task SeedRolesAsync()
    {
        foreach (string roleName in AuthRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new Role(roleName));
            }
        }
    }

    private async Task UpsertScopeAsync(
        string name,
        string displayName,
        string resource,
        CancellationToken cancellationToken)
    {
        var descriptor = new OpenIddictScopeDescriptor
        {
            Name = name,
            DisplayName = displayName,
            Resources = { resource },
        };

        object? existing = await scopeManager.FindByNameAsync(descriptor.Name, cancellationToken);
        if (existing is null)
        {
            await scopeManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await scopeManager.UpdateAsync(existing, descriptor, cancellationToken);
        }
    }

    private async Task UpsertPublicClientAsync(
        OidcClientOptions client,
        bool allowPasswordGrant,
        CancellationToken cancellationToken)
    {
        if (!client.IsConfigured)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientType = client.IsPublic
                ? OpenIddictConstants.ClientTypes.Public
                : OpenIddictConstants.ClientTypes.Confidential,
            ConsentType = OpenIddictConstants.ConsentTypes.Implicit,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.Endpoints.EndSession,
                OpenIddictConstants.Permissions.Endpoints.Revocation,
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
                OpenIddictConstants.Permissions.Scopes.Email,
                OpenIddictConstants.Permissions.Scopes.Profile,
                OpenIddictConstants.Permissions.Scopes.Roles,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.AUTH,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.CLANS,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.PROFILES,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.WALLET,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.CATALOG,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.INVENTORY,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.STORE,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.EVENTS,
                OpenIddictConstants.Permissions.Prefixes.Scope + OidcScopes.REALTIME,
            },
        };

        if (!client.IsPublic)
        {
            descriptor.ClientSecret = client.ClientSecret;
        }

        if (allowPasswordGrant)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
        }

        foreach (string redirectUri in client.RedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        foreach (string postLogoutRedirectUri in client.PostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(postLogoutRedirectUri));
        }

        await UpsertApplicationAsync(client.ClientId, descriptor, cancellationToken);
    }

    private async Task UpsertServiceClientAsync(
        OidcClientOptions client,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken)
    {
        if (!client.IsConfigured)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = client.ClientSecret,
            DisplayName = client.DisplayName,
            ClientType = OpenIddictConstants.ClientTypes.Confidential,
            Permissions =
            {
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
            },
        };

        foreach (string scope in scopes)
        {
            descriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
        }

        await UpsertApplicationAsync(client.ClientId, descriptor, cancellationToken);
    }

    private async Task UpsertApplicationAsync(
        string clientId,
        OpenIddictApplicationDescriptor descriptor,
        CancellationToken cancellationToken)
    {
        object? existing = await applicationManager.FindByClientIdAsync(clientId, cancellationToken);
        if (existing is null)
        {
            await applicationManager.CreateAsync(descriptor, cancellationToken);
        }
        else
        {
            await applicationManager.UpdateAsync(existing, descriptor, cancellationToken);
        }
    }
}
