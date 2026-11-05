using Scalar.AspNetCore;
using Shared.Authentication;
using Shared.Framework.Cors;
using Shared.Framework.Endpoints;
using Shared.Framework.Logging;
using Shared.Framework.Middlewares;

namespace ProfileService.Web.Configurations;

public static class AppConfigurationExtensions
{
    public static IApplicationBuilder Configure(this WebApplication app)
    {
        app.UseRouting();
        app.ConfigureCors();
        app.UseSerilogHttpRequestLogging();
        app.UseExceptionMiddleware();
        app.UseRequestCorrelationId();
        app.UsePlatformAuthentication();

        if (!app.Environment.IsProduction())
        {
            app.MapOpenApi().AllowAnonymous();
            app.MapScalarApiReference().AllowAnonymous();
        }

        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapEndpoints();
        app.MapEndpointGroups(typeof(AppConfigurationExtensions).Assembly);

        return app;
    }
}
