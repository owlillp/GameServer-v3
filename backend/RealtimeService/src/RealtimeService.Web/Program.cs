using Serilog;
using Shared.Framework.Logging;
using RealtimeService.Web.Configurations;

Log.Logger = LoggingExtensions.CreateBootstrapLogger(Constants.SERVICE_NAME);

try
{
    Log.Information("Starting application: {ServiceName}", Constants.SERVICE_NAME);

    var builder = WebApplication.CreateBuilder(args);

    builder.AddDependency(builder.Configuration);

    var app = builder.Build();

    app.Configure();

    await app.RunAsync();
}
catch (HostAbortedException)
{
    // Expected when running EF migrations tooling
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

namespace RealtimeService.Web
{
    public class Program;
}
