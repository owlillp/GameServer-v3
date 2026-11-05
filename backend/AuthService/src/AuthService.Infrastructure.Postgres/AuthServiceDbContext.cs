using AuthService.Core.Abstractions;
using AuthService.Domain.Accounts;
using AuthService.Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace AuthService.Infrastructure.Postgres;

public class AuthServiceDbContext(DbContextOptions<AuthServiceDbContext> options)
    : IdentityDbContext<Account, Role, Guid>(options), IReadDbContext
{
    public DbSet<Account> Accounts => Set<Account>();

    public IQueryable<Account> AccountsRead => Set<Account>()
        .AsQueryable()
        .AsNoTracking();

    public IQueryable<Role> RolesRead => Set<Role>()
        .AsQueryable()
        .AsNoTracking();

    public IQueryable<IdentityUserRole<Guid>> AccountRolesRead => Set<IdentityUserRole<Guid>>()
        .AsQueryable()
        .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema(Constants.SCHEMA);
        builder.UseOpenIddict();
        builder.MapWolverineEnvelopeStorage("public");
        builder.ApplyConfigurationsFromAssembly(typeof(AuthServiceDbContext).Assembly);
    }
}
