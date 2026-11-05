using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.Infrastructure.Postgres;

public class AuthServiceDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AuthServiceDbContext>
{
    private const string CONNECTION_STRING = "Host=design-time;Database=design;Username=postgres";

    public AuthServiceDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable(
                                    $"ConnectionStrings__{AuthService.Core.Constants.ConnectionStringNames.DATABASE}")
                                  ?? CONNECTION_STRING;

        var optionsBuilder = new DbContextOptionsBuilder<AuthServiceDbContext>();
        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable(Constants.EF_MIGRATION_HISTORY, Constants.SCHEMA));

        return new AuthServiceDbContext(optionsBuilder.Options);
    }
}
