using AuthService.Core.Abstractions;
using AuthService.Core.Messaging;
using AuthService.Domain.Accounts;
using AuthService.Domain.Authorization;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared.Core.Database;
using Wolverine;

namespace AuthService.Infrastructure.Postgres;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructurePostgres(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        string connectionName = AuthService.Core.Constants.ConnectionStringNames.DATABASE;

        string connectionString = configuration.GetConnectionString(connectionName)
                                  ?? throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            { Name = connectionName }.Build();

        services.AddSingleton(dataSource);
        services.AddDbContextPool<AuthServiceDbContext>((sp, options) =>
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

        services.AddScoped<ITransactionManager, TransactionManager>();
        services.AddScoped<IReadDbContext, AuthServiceDbContext>();
        services.AddScoped<IOutboxService, OutboxService>();

        services.AddSingleton<IWolverineExtension, AuthWolverineExtension>();

        new IdentityBuilder(typeof(Account), typeof(Role), services)
            .AddEntityFrameworkStores<AuthServiceDbContext>();

        return services;
    }
}
