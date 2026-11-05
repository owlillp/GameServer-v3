using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AuthService.Contracts.Admin;
using AuthService.Domain.Authorization;
using AuthService.IntegrationTests.Infrastructure;
using Shared.SharedKernel;
using Shared.SharedKernel.Responses;

namespace AuthService.IntegrationTests.Features.Admin;

public sealed class AdminEndpointsTests : IntegrationTestsBase
{
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public AdminEndpointsTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task Users_Anonymous_IsUnauthorized()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.GetAsync("/admin/users?page=1&pageSize=20");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Users_RegularUser_IsForbidden()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("regular@test.com", PASSWORD);
        string accessToken = await GetUserAccessTokenAsync("regular@test.com");

        HttpResponseMessage response = await GetAuthorizedAsync("/admin/users?page=1&pageSize=20", accessToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Users_ServiceToken_IsForbidden()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse tokens = await _oidc.ExecuteClientCredentialsFlowAsync(
            IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_ID,
            IntegrationTestsWebFactory.CLAN_SERVICE_CLIENT_SECRET,
            "auth");

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        HttpResponseMessage response = await GetAuthorizedAsync("/admin/users?page=1&pageSize=20", tokens.AccessToken!);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Users_Moderator_SeesRegisteredUsers()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("moderator@test.com", PASSWORD, "moderatoruser");
        await RegisterAsync("player1@test.com", PASSWORD, "playerone");
        await RegisterAsync("player2@test.com", PASSWORD, "playertwo");
        await AddRoleAsync("moderator@test.com", AuthRoles.MODERATOR);

        string accessToken = await GetUserAccessTokenAsync("moderator@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/admin/users?page=1&pageSize=20", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<PaginationResponse<AdminUserDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<PaginationResponse<AdminUserDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(3, envelope.Result.TotalCount);

        AdminUserDto moderator = envelope.Result.Items.Single(user => string.Equals(user.Email, "moderator@test.com", StringComparison.Ordinal));
        Assert.Contains(AuthRoles.MODERATOR, moderator.Roles, StringComparer.Ordinal);
        Assert.Contains(AuthRoles.USER, moderator.Roles, StringComparer.Ordinal);
    }

    [Fact]
    public async Task Stats_Moderator_ReturnsRoleCounts()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("admin@test.com", PASSWORD, "adminuser");
        await RegisterAsync("moderator@test.com", PASSWORD, "moderatoruser");
        await RegisterAsync("player@test.com", PASSWORD, "playeruser");
        await AddRoleAsync("admin@test.com", AuthRoles.ADMIN);
        await AddRoleAsync("moderator@test.com", AuthRoles.MODERATOR);

        string accessToken = await GetUserAccessTokenAsync("moderator@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/admin/stats", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<AdminStatsDto>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<AdminStatsDto>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(3, envelope.Result.TotalUsers);
        Assert.Equal(1, envelope.Result.AdminCount);
        Assert.Equal(1, envelope.Result.ModeratorCount);
        Assert.Equal(0, envelope.Result.LockedOutCount);
    }

    [Fact]
    public async Task Users_Search_FiltersByEmail()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync("admin@test.com", PASSWORD, "adminuser");
        await RegisterAsync("needle@test.com", PASSWORD, "needleuser");
        await RegisterAsync("other@test.com", PASSWORD, "otheruser");
        await AddRoleAsync("admin@test.com", AuthRoles.ADMIN);

        string accessToken = await GetUserAccessTokenAsync("admin@test.com");
        HttpResponseMessage response = await GetAuthorizedAsync("/admin/users?page=1&pageSize=20&search=needle", accessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Envelope<PaginationResponse<AdminUserDto>>? envelope =
            await response.Content.ReadFromJsonAsync<Envelope<PaginationResponse<AdminUserDto>>>();

        Assert.NotNull(envelope);
        Assert.NotNull(envelope.Result);
        Assert.Equal(1, envelope.Result.TotalCount);
        Assert.Equal("needle@test.com", envelope.Result.Items[0].Email);
    }

    private async Task<HttpResponseMessage> GetAuthorizedAsync(string url, string accessToken)
    {
        using HttpClient client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await client.GetAsync(url);
    }

    private async Task<string> GetUserAccessTokenAsync(string email)
    {
        await _oidc.LoginAsync(email, PASSWORD);

        OidcTokenResponse tokens = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(tokens.IsSuccess, tokens.RawResponse);

        return tokens.AccessToken!;
    }
}
