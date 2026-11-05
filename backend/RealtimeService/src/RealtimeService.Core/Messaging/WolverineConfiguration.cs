using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Shared.Messaging;
using Wolverine;

namespace RealtimeService.Core.Messaging;

public static class WolverineConfiguration
{
    public static void AddWolverine(this WebApplicationBuilder builder)
    {
        string rabbitConnectionString = builder.Configuration.GetConnectionString(Constants.ConnectionStringNames.RABBIT_MQ)
            ?? throw new InvalidOperationException($"Connection string '{Constants.ConnectionStringNames.RABBIT_MQ}' is not configured.");

        builder.Host.UseWolverine(
            options =>
            {
                options.ApplicationAssembly = typeof(WolverineConfiguration).Assembly;

                options.UseRuntimeCompilation();
                options.ConfigureRabbitMq(rabbitConnectionString);
                options.ConfigureStandardErrorPolicies();
            }, ExtensionDiscovery.ManualOnly);
    }
}
