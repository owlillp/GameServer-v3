using Microsoft.EntityFrameworkCore;
using ProfileService.Core.Abstractions;
using ProfileService.Domain.Players;
using Wolverine.EntityFrameworkCore;

namespace ProfileService.Infrastructure.Postgres;

public class ProfileServiceDbContext(DbContextOptions<ProfileServiceDbContext> options)
    : DbContext(options), IReadDbContext
{
    public DbSet<PlayerProfile> Profiles => Set<PlayerProfile>();

    public IQueryable<PlayerProfile> ProfilesRead => Set<PlayerProfile>()
        .AsQueryable()
        .AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Constants.SCHEMA);
        modelBuilder.MapWolverineEnvelopeStorage("public");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProfileServiceDbContext).Assembly);
    }
}
