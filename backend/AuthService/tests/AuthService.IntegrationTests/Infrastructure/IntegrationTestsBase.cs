using System.Net.Http.Json;
using AuthService.Contracts.Requests;
using AuthService.Core.Configuration;
using AuthService.Core.Services;
using AuthService.Domain.Accounts;
using AuthService.Infrastructure.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AuthService.IntegrationTests.Infrastructure;

[Collection(IntegrationTestFixture.Name)]
public abstract class IntegrationTestsBase : IAsyncLifetime
{
    protected IntegrationTestsBase(IntegrationTestsWebFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        Services = factory.Services;
    }

    protected IntegrationTestsWebFactory Factory { get; }

    protected HttpClient HttpClient { get; }

    protected IServiceProvider Services { get; }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => Factory.ResetDatabaseAsync();

    protected async Task SeedPlatformConfigAsync()
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        PlatformSyncService syncService = scope.ServiceProvider
            .GetRequiredService<PlatformSyncService>();
        OidcOptions options = scope.ServiceProvider
            .GetRequiredService<IOptions<OidcOptions>>().Value;

        await syncService.SyncAsync(options, CancellationToken.None);
    }

    protected async Task RegisterAsync(string email, string password, string userName = "testuser")
    {
        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(email, userName, password));

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Registration failed: {response.StatusCode} — {await response.Content.ReadAsStringAsync()}");
        }
    }

    protected async Task<Guid> GetAccountIdAsync(string email)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        UserManager<Account> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

        Account? account = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"Account '{email}' was not found.");

        return account.Id;
    }

    protected async Task AddRoleAsync(string email, string role)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        UserManager<Account> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

        Account account = await userManager.FindByEmailAsync(email)
            ?? throw new InvalidOperationException($"Account '{email}' was not found.");

        if (await userManager.IsInRoleAsync(account, role))
        {
            return;
        }

        IdentityResult result = await userManager.AddToRoleAsync(account, role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to add role '{role}': {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }
    }

    protected async Task RotateSecurityStampAsync(Guid accountId)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        UserManager<Account> userManager = scope.ServiceProvider.GetRequiredService<UserManager<Account>>();

        Account account = await userManager.FindByIdAsync(accountId.ToString())
            ?? throw new InvalidOperationException($"Account '{accountId}' was not found.");

        IdentityResult result = await userManager.UpdateSecurityStampAsync(account);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to rotate security stamp: {string.Join(", ", result.Errors.Select(error => error.Description))}");
        }
    }

    protected async Task<TResult> ExecuteInDbAsync<TResult>(Func<AuthServiceDbContext, Task<TResult>> action)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AuthServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthServiceDbContext>();

        return await action(dbContext);
    }
}
