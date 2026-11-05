using System.Threading.RateLimiting;
using AuthService.Core;
using AuthService.Core.Messaging;
using AuthService.Domain.Authorization;
using AuthService.Infrastructure.Postgres;
using AuthService.Web.Endpoints.OidcConnection;
using Shared.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.OpenApi;
using Shared.Messaging;

namespace AuthService.Web.Configurations;

public static class DependencyInjectionExtensions
{
    private const string POSTGRESQL_HEALTH_CHECK = "postgresql";
    private const string AUTHENTICATION_SECTION_NAME = "Authentication";

    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddDependency(
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            builder.AddSerilogLogging(Constants.SERVICE_NAME);
            builder.AddRateLimiter();
            builder.AddWolverine();

            builder.Services.AddPlatformAuthentication(configuration, RolePermissions.Map, AUTHENTICATION_SECTION_NAME);
            builder.Services.AddFrameworkCors(configuration);
            builder.Services.AddOpenApiSpec(Constants.SERVICE_NAME, Constants.VERSION);
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AuthServiceDbContext>(POSTGRESQL_HEALTH_CHECK)
                .AddRabbitMqCheck(configuration);
            builder.Services.AddEndpoints(typeof(DependencyInjectionExtensions).Assembly);

            builder.Services.AddCore(configuration);
            builder.Services.AddInfrastructurePostgres(configuration);

            builder.Services.AddAuthServiceOidc(configuration, environment);

            return builder;
        }

        private void AddRateLimiter() =>
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(ConnectConstants.TOKEN_RATE_LIMIT_POLICY, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 120,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                        }));
            });
    }
}
