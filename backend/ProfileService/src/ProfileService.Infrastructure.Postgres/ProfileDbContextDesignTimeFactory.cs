using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProfileService.Infrastructure.Postgres;

public class ProfileDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ProfileServiceDbContext>
{
    private const string CONNECTION_STRING = "Host=design-time;Database=design;Username=postgres";

    public ProfileServiceDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable(
                $"ConnectionStrings__{ProfileService.Core.Constants.ConnectionStringNames.DATABASE}")
            ?? CONNECTION_STRING;

        var optionsBuilder = new DbContextOptionsBuilder<ProfileServiceDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable(Constants.EF_MIGRATION_HISTORY, Constants.SCHEMA));

        return new ProfileServiceDbContext(optionsBuilder.Options);
    }
}
