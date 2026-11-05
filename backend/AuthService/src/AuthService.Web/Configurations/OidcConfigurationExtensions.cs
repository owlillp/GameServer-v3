using System.Security.Cryptography;
using System.Text;
using AuthService.Core.Configuration;
using AuthService.Infrastructure.Postgres;
using AuthService.Web.Configurations.Options;
using AuthService.Web.Endpoints.OidcConnection;
using AuthService.Web.Seeding;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;

namespace AuthService.Web.Configurations;
public static class OidcConfigurationExtensions
{
    private static bool IsLocalInsecureEnvironment(IHostEnvironment environment)
        => environment.IsDevelopment() || environment.IsEnvironment("Docker");

    public static IServiceCollection AddAuthServiceOidc(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.Configure<OidcOptions>(configuration.GetSection(OidcOptions.SECTION_NAME));
        services.Configure<SigningKeyOptions>(configuration.GetSection(SigningKeyOptions.SECTION_NAME));
        services.Configure<AuthServiceOptions>(configuration.GetSection(AuthServiceOptions.SECTION_NAME));

        var oidcOptions = configuration
            .GetSection(OidcOptions.SECTION_NAME)
            .Get<OidcOptions>() ?? new OidcOptions();

        var signingKeyOptions = configuration
            .GetSection(SigningKeyOptions.SECTION_NAME)
            .Get<SigningKeyOptions>() ?? new SigningKeyOptions();

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<AuthServiceDbContext>()
            ).AddServer(options =>
            {
                if (!string.IsNullOrWhiteSpace(oidcOptions.Issuer))
                {
                    options.SetIssuer(new Uri(oidcOptions.Issuer));
                }

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();
                options.AllowClientCredentialsFlow();
                options.AllowPasswordFlow();

                options.RequireProofKeyForCodeExchange();
                options.DisableAccessTokenEncryption();

                options.SetAuthorizationEndpointUris(ConnectConstants.AUTHORIZE_ENDPOINT);
                options.SetTokenEndpointUris(ConnectConstants.TOKEN_ENDPOINT);
                options.SetRevocationEndpointUris(ConnectConstants.REVOKE_ENDPOINT);
                options.SetUserInfoEndpointUris(ConnectConstants.USER_INFO_ENDPOINT);
                options.SetEndSessionEndpointUris(ConnectConstants.END_SESSION_ENDPOINT);

                options.RegisterScopes([.. OidcScopes.All]);

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(oidcOptions.AccessTokenLifetimeMinutes));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(oidcOptions.RefreshTokenLifetimeDays));
                options.SetRefreshTokenReuseLeeway(TimeSpan.Zero);

                AddSigningKeys(options, environment, signingKeyOptions);

                var aspNetCore = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();

                if (IsLocalInsecureEnvironment(environment))
                {
                    aspNetCore.DisableTransportSecurityRequirement();
                }
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        });

        services.AddHostedService<OidcSeeder>();

        return services;
    }

    private static void AddSigningKeys(
        OpenIddictServerBuilder builder,
        IWebHostEnvironment environment,
        SigningKeyOptions keys)
    {
        if (keys.IsConfigured)
        {
            builder.AddSigningKey(ImportRsaKey(keys.SigningKeyBase64!));
            builder.AddEncryptionKey(ImportRsaKey(keys.EncryptionKeyBase64!));
            return;
        }

        if (environment.IsProduction())
        {
            throw new InvalidOperationException(
                "Production signing/encryption keys are required. Set SigningKeys:SigningKeyBase64 and SigningKeys:EncryptionKeyBase64.");
        }

        builder.AddDevelopmentSigningCertificate();
        builder.AddDevelopmentEncryptionCertificate();
    }

    private static RsaSecurityKey ImportRsaKey(string base64Pem)
    {
        byte[] pem = Convert.FromBase64String(base64Pem);
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(Encoding.UTF8.GetString(pem));
        return new RsaSecurityKey(rsa);
    }
}