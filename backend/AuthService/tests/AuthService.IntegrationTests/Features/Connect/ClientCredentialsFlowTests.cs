using System.Net;
using AuthService.Domain.Authorization;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class ClientCredentialsFlowTests : IntegrationTestsBase
{
    private readonly OidcTestHelper _oidc;

    public ClientCredentialsFlowTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task ServiceClient_ReturnsAccessTokenWithServiceRole()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse response = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_ID,
            IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_SECRET,
            "auth");

        Assert.True(response.IsSuccess, response.RawResponse);
        Assert.NotNull(response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.True(response.ExpiresIn > 0);

        JwtClaims claims = OidcTestHelper.ParseJwt(response.AccessToken);

        Assert.Equal(IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_ID, claims.GetString("sub"));
        Assert.Contains(AuthRoles.SERVICE, claims.GetValues("role"), StringComparer.Ordinal);
        Assert.Contains("auth-service", claims.GetValues("aud"), StringComparer.Ordinal);
    }

    [Fact]
    public async Task InvalidSecret_IsRejected()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse response = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_ID,
            "wrong-secret",
            "auth");

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UnknownClient_IsRejected()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse response = await _oidc.ExecuteClientCredentialsFlowAsync(
            "unknown-client",
            "unknown-secret",
            "auth");

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
