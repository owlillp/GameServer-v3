using AuthService.Infrastructure.Postgres;
using AuthService.Web.Endpoints.OidcConnection;
using AuthService.Web.Seeding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Npgsql;
using OpenIddict.Server.AspNetCore;
using Respawn;
using System.Threading.RateLimiting;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace AuthService.IntegrationTests.Infrastructure;

public sealed class IntegrationTestsWebFactory : WebApplicationFactory<AuthService.Web.Program>, IAsyncLifetime
{
    public const string WEB_CLIENT_ID = "gameserver-v3-web-tests";
    public const string WEB_REDIRECT_URI = "http://localhost/auth/callback";
    public const string UNITY_CLIENT_ID = "gameserver-v3-unity-tests";
    public const string UNITY_REDIRECT_URI = "http://127.0.0.1:7777/callback";
    public const string UNITY_CUSTOM_SCHEME_REDIRECT_URI = "gameserver-v3://auth/callback";
    public const string CLAN_SERVICE_CLIENT_ID = "clan-service-client-tests";
    public const string CLAN_SERVICE_CLIENT_SECRET = "test-clan-service-secret-0123456789";
    public const string FRONTEND_LOGIN_URL = "http://localhost/login";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("auth_service_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:4-management").Build();

    private NpgsqlConnection _connection = null!;
    private Respawner _respawner = null!;

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_database.StartAsync(), _rabbitMq.StartAsync());
        ConnectionString = _database.GetConnectionString();

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        AuthServiceDbContext dbContext = scope.ServiceProvider.GetRequiredService<AuthServiceDbContext>();
        await dbContext.Database.MigrateAsync();

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = [AuthService.Infrastructure.Postgres.Constants.SCHEMA],
        });
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _database.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }

    public Task ResetDatabaseAsync() => _respawner.ResetAsync(_connection);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // UseSetting (unlike ConfigureAppConfiguration) применяется до того,
        // как Program читает конфигурацию при регистрации DI.
        builder.UseSetting("ConnectionStrings:AuthDb", ConnectionString);
        builder.UseSetting("ConnectionStrings:RabbitMQ", _rabbitMq.GetConnectionString());
        builder.UseSetting("Authentication:Authority", "http://localhost/");
        builder.UseSetting("Authentication:Audiences:0", "auth-service");
        builder.UseSetting("OpenIddict:Issuer", "http://localhost/");
        builder.UseSetting("OpenIddict:AccessTokenLifetimeMinutes", "5");
        builder.UseSetting("OpenIddict:RefreshTokenLifetimeDays", "7");
        builder.UseSetting("OpenIddict:Clients:Web:ClientId", WEB_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:Web:DisplayName", "Test Web Client");
        builder.UseSetting("OpenIddict:Clients:Web:RedirectUris:0", WEB_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:Web:PostLogoutRedirectUris:0", "http://localhost/");
        builder.UseSetting("OpenIddict:Clients:Unity:ClientId", UNITY_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:Unity:DisplayName", "Test Unity Client");
        builder.UseSetting("OpenIddict:Clients:Unity:RedirectUris:0", UNITY_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:Unity:RedirectUris:1", UNITY_CUSTOM_SCHEME_REDIRECT_URI);
        builder.UseSetting("OpenIddict:Clients:ClanService:ClientId", CLAN_SERVICE_CLIENT_ID);
        builder.UseSetting("OpenIddict:Clients:ClanService:ClientSecret", CLAN_SERVICE_CLIENT_SECRET);
        builder.UseSetting("OpenIddict:Clients:ClanService:DisplayName", "Test ClanService Client");
        builder.UseSetting("AuthService:FrontendLoginUrl", FRONTEND_LOGIN_URL);

        builder.ConfigureTestServices(services =>
        {
            RemoveHostedService<OidcSeeder>(services);

            // Rate limiter не должен влиять на тесты: политика без лимита.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.Configure<RateLimiterOptions>(options =>
                options.AddPolicy(
                    ConnectConstants.TOKEN_RATE_LIMIT_POLICY,
                    _ => RateLimitPartition.GetNoLimiter(string.Empty)));

            services.Configure<OpenIddictServerAspNetCoreOptions>(options =>
                options.DisableTransportSecurityRequirement = true);
        });
    }

    private static void RemoveHostedService<TImplementation>(IServiceCollection services)
    {
        foreach (ServiceDescriptor descriptor in services
                     .Where(descriptor => descriptor.ImplementationType == typeof(TImplementation))
                     .ToList())
        {
            services.Remove(descriptor);
        }
    }
}
