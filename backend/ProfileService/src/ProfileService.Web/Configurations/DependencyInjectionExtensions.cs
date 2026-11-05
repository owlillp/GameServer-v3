using ProfileService.Core;
using ProfileService.Core.Messaging;
using ProfileService.Domain.Authorization;
using ProfileService.Infrastructure.Postgres;
using Shared.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.OpenApi;
using Shared.Messaging;

namespace ProfileService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string POSTGRESQL_HEALTH_CHECK = "postgresql";
    private const string AUTHENTICATION_SECTION_NAME = "Authentication";

    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddDependency(IConfiguration configuration)
        {
            builder.AddSerilogLogging(Constants.SERVICE_NAME);
            builder.AddWolverine();

            builder.Services.AddPlatformAuthentication(configuration, RolePermissions.Map, AUTHENTICATION_SECTION_NAME);
            builder.Services.AddFrameworkCors(configuration);
            builder.Services.AddOpenApiSpec(Constants.SERVICE_NAME, Constants.VERSION);
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<ProfileServiceDbContext>(POSTGRESQL_HEALTH_CHECK)
                .AddRabbitMqCheck(configuration);
            builder.Services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

            builder.Services.AddCore();
            builder.Services.AddInfrastructurePostgres(configuration);

            return builder;
        }
    }
}
