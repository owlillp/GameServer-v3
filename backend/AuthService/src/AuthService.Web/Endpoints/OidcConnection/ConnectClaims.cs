using System.Security.Claims;
using AuthService.Domain.Accounts;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using Shared.Authentication.Authentication;

namespace AuthService.Web.Endpoints.OidcConnection;

internal static class ConnectClaims
{
    public static ClaimsIdentity CreateUserIdentity(Account user, IEnumerable<string> roles)
    {
        var identity = new ClaimsIdentity(
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
        identity.SetClaim(OpenIddictConstants.Claims.Name, user.UserName ?? string.Empty);
        identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email ?? string.Empty);
        identity.SetClaim(OpenIddictConstants.Claims.PreferredUsername, user.UserName ?? string.Empty);

        foreach (string role in roles)
        {
            identity.AddClaim(new Claim(OpenIddictConstants.Claims.Role, role));
        }

        if (!string.IsNullOrEmpty(user.SecurityStamp))
        {
            identity.SetClaim(AuthClaimTypes.SECURITY_STAMP, user.SecurityStamp);
        }

        return identity;
    }

    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);

        return claim.Type switch
        {
            OpenIddictConstants.Claims.Subject
                or OpenIddictConstants.Claims.Name
                or OpenIddictConstants.Claims.Email
                or OpenIddictConstants.Claims.PreferredUsername
                or OpenIddictConstants.Claims.Role
                =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken,
                ],

            AuthClaimTypes.SECURITY_STAMP => [],

            _ => [OpenIddictConstants.Destinations.AccessToken],
        };
    }
}