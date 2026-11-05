using RealtimeService.Core;
using RealtimeService.Core.Configuration;
using RealtimeService.Core.Messaging;
using RealtimeService.Infrastructure.Redis;
using RealtimeService.Web.Hubs;
using RealtimeService.Web.Services;
using Shared.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.OpenApi;
using Shared.Messaging;

namespace RealtimeService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string AUTHENTICATION_SECTION_NAME = "Authentication";

    private static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> EMPTY_PERMISSIONS =
        new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);

    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddDependency(IConfiguration configuration)
        {
            string redisConnectionString = configuration.GetConnectionString(
                    RealtimeService.Core.Constants.ConnectionStringNames.REDIS)
                ?? throw new InvalidOperationException("Connection string 'Redis' is not configured.");

            builder.AddSerilogLogging(Constants.SERVICE_NAME);
            builder.AddWolverine();

            builder.Services.AddPlatformAuthentication(
                configuration,
                EMPTY_PERMISSIONS,
                AUTHENTICATION_SECTION_NAME);
            builder.Services.AddFrameworkCors(configuration);
            builder.Services.AddOpenApiSpec(Constants.SERVICE_NAME, Constants.VERSION);
            builder.Services.AddHealthChecks()
                .AddRabbitMqCheck(configuration)
                .AddCheck<RedisHealthCheck>("redis");
            builder.Services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

            builder.Services.Configure<RealtimeOptions>(
                configuration.GetSection(RealtimeOptions.SECTION_NAME));

            builder.Services.AddSignalR()
                .AddStackExchangeRedis(redisConnectionString);

            builder.Services.AddSingleton<IRealtimePublisher, SignalRRealtimePublisher>();

            builder.Services.AddCore();
            builder.Services.AddInfrastructureRedis(configuration);

            return builder;
        }
    }
}
