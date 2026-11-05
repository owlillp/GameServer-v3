using System.Net;
using System.Text.Json;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class DiscoveryTests : IntegrationTestsBase
{
    public DiscoveryTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task OpenIdConfiguration_ExposesEndpointsAndScopes()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.GetAsync("/.well-known/openid-configuration");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement root = document.RootElement;

        Assert.Equal("http://localhost/", root.GetProperty("issuer").GetString());
        Assert.Equal("http://localhost/connect/authorize", root.GetProperty("authorization_endpoint").GetString());
        Assert.Equal("http://localhost/connect/token", root.GetProperty("token_endpoint").GetString());
        Assert.Equal("http://localhost/connect/userinfo", root.GetProperty("userinfo_endpoint").GetString());
        Assert.Equal("http://localhost/connect/revoke", root.GetProperty("revocation_endpoint").GetString());

        IReadOnlyList<string> scopes = root.GetProperty("scopes_supported")
            .EnumerateArray()
            .Select(scope => scope.GetString()!)
            .ToArray();

        Assert.Contains("openid", scopes, StringComparer.Ordinal);
        Assert.Contains("offline_access", scopes, StringComparer.Ordinal);
        Assert.Contains("auth", scopes, StringComparer.Ordinal);
        Assert.Contains("clans", scopes, StringComparer.Ordinal);
    }
}
