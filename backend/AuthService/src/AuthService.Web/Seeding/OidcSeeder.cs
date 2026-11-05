using AuthService.Core.Configuration;
using AuthService.Core.Services;
using Microsoft.Extensions.Options;

namespace AuthService.Web.Seeding;

public sealed class OidcSeeder(
    IServiceProvider serviceProvider,
    IOptions<OidcOptions> options,
    ILogger<OidcSeeder> logger) : BackgroundService
{
    private static readonly TimeSpan _retryDelay = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = serviceProvider.CreateAsyncScope();
                var syncService = scope.ServiceProvider.GetRequiredService<PlatformSyncService>();

                await syncService.SyncAsync(options.Value, stoppingToken);

                logger.LogInformation("Oidc configuration synchronized");
                return;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to synchronize Oidc configuration, retrying in {Delay}", _retryDelay);
                await Task.Delay(_retryDelay, stoppingToken);
            }
        }
    }
}
