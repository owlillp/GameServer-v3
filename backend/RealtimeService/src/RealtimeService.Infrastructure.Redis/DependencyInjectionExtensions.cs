using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RealtimeService.Core.Abstractions;
using StackExchange.Redis;

namespace RealtimeService.Infrastructure.Redis;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddInfrastructureRedis(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionName = Core.Constants.ConnectionStringNames.REDIS;
        string connectionString = configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException($"Connection string '{connectionName}' is not configured.");

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;

            return ConnectionMultiplexer.Connect(options);
        });

        services.AddScoped<ITicketStore, RedisTicketStore>();

        return services;
    }
}
