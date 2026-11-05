using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Extensions;

public static class SignInManagerExtensions
{
    public static Task SignInWithClaimsAsync<TUser>(
        this SignInManager<TUser> signInManager,
        IReadOnlyCollection<Claim> claims,
        bool isPersistent)
        where TUser : class
    {
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(claims);

        var identity = new ClaimsIdentity(
            claims,
            IdentityConstants.ApplicationScheme,
            signInManager.Options.ClaimsIdentity.UserNameClaimType,
            signInManager.Options.ClaimsIdentity.RoleClaimType);

        var principal = new ClaimsPrincipal(identity);

        return signInManager.Context.SignInAsync(
            IdentityConstants.ApplicationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = isPersistent });
    }
}