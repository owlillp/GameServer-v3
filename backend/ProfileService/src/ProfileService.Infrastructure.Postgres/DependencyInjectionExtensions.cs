using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using ProfileService.Core.Abstractions;
using ProfileService.Core.Messaging;
using ProfileService.Infrastructure.Postgres.Repositories;
using Shared.Core.Database;
using Wolverine;

namespace ProfileService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionName = ProfileService.Core.Constants.ConnectionStringNames.DATABASE;

        string connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            { Name = connectionName }.Build();

        services.AddSingleton(dataSource);
        services.AddDbContextPool<ProfileServiceDbContext>((sp, options) =>
        {
            var hostEnvironment = sp.GetRequiredService<IHostEnvironment>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            options.UseNpgsql(
                dataSource,
                npgsql => npgsql.MigrationsHistoryTable(Constants.EF_MIGRATION_HISTORY, Constants.SCHEMA));
            options.UseLoggerFactory(loggerFactory);

            if (hostEnvironment.IsDevelopment())
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IReadDbContext, ProfileServiceDbContext>();
        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IOutboxService, OutboxService>();

        services.AddScoped<IProfileRepository, ProfileRepository>();

        services.AddSingleton<IWolverineExtension, ProfileWolverineExtension>();

        return services;
    }
}
