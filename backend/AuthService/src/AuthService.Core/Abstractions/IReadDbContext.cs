using AuthService.Domain.Accounts;
using AuthService.Domain.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<Account> AccountsRead { get; }

    IQueryable<Role> RolesRead { get; }

    IQueryable<IdentityUserRole<Guid>> AccountRolesRead { get; }
}