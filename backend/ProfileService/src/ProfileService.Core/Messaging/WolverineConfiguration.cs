using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Shared.Messaging;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;

namespace ProfileService.Core.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitConnectionString = builder.Configuration.GetConnectionString(Constants.ConnectionStringNames.RABBIT_MQ)
            ?? throw new InvalidOperationException($"Connection string '{Constants.ConnectionStringNames.RABBIT_MQ}' is not configured.");
        string databaseConnectionString = builder.Configuration.GetConnectionString(Constants.ConnectionStringNames.DATABASE)
            ?? throw new InvalidOperationException($"Connection string '{Constants.ConnectionStringNames.DATABASE}' is not configured.");

        builder.Host.UseWolverine(
            options =>
            {
                options.ApplicationAssembly = typeof(WolverineConfiguration).Assembly;

                options.UseRuntimeCompilation();
                options.ConfigureDurableMessaging(databaseConnectionString);
                options.ConfigureRabbitMq(rabbitConnectionString);
                options.ConfigureStandardErrorPolicies();
            }, ExtensionDiscovery.ManualOnly);
    }

    private static void ConfigureDurableMessaging(this WolverineOptions options, string connectionString)
    {
        options.PersistMessagesWithPostgresql(connectionString, "public");
        options.UseEntityFrameworkCoreTransactions();
        options.Policies.UseDurableOutboxOnAllSendingEndpoints();
        options.Policies.UseDurableInboxOnAllListeners();
    }
}