using System.Net;
using AuthService.Domain.Authorization;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

// In-game логин Unity (grant_type=password для public-клиента).
public sealed class PasswordGrantTests : IntegrationTestsBase
{
    private const string EMAIL = "unity-player@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public PasswordGrantTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task PasswordGrant_ValidCredentials_ReturnsTokensWithUserClaims()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityplayer");

        OidcTokenResponse response = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            EMAIL,
            PASSWORD,
            SCOPE);

        Assert.True(response.IsSuccess, response.RawResponse);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.RefreshToken);
        Assert.Equal("Bearer", response.TokenType);

        JwtClaims claims = OidcTestHelper.ParseJwt(response.AccessToken);
        Assert.Equal(EMAIL, claims.GetString("email"));
        Assert.Contains(AuthRoles.USER, claims.GetValues("role"), StringComparer.Ordinal);
        Assert.Contains("auth-service", claims.GetValues("aud"), StringComparer.Ordinal);
    }

    [Fact]
    public async Task PasswordGrant_RefreshToken_RenewsSession()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityplayer");

        OidcTokenResponse initial = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            EMAIL,
            PASSWORD,
            SCOPE);

        Assert.True(initial.IsSuccess, initial.RawResponse);
        Assert.NotNull(initial.RefreshToken);

        // Следующий запуск игры: refresh без пароля.
        OidcTokenResponse refreshed = await _oidc.RefreshTokenAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            clientSecret: null,
            initial.RefreshToken);

        Assert.True(refreshed.IsSuccess, refreshed.RawResponse);
        Assert.NotNull(refreshed.AccessToken);

        JwtClaims claims = OidcTestHelper.ParseJwt(refreshed.AccessToken);
        Assert.Equal(EMAIL, claims.GetString("email"));
    }

    [Fact]
    public async Task PasswordGrant_RefreshAfterSecurityStampRotation_IsRejected()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityplayer");

        OidcTokenResponse initial = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            EMAIL,
            PASSWORD,
            SCOPE);

        Assert.True(initial.IsSuccess, initial.RawResponse);
        Assert.NotNull(initial.RefreshToken);

        Guid accountId = await GetAccountIdAsync(EMAIL);
        await RotateSecurityStampAsync(accountId);

        // Security stamp изменился — старый refresh token недействителен.
        OidcTokenResponse refreshed = await _oidc.RefreshTokenAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            clientSecret: null,
            initial.RefreshToken);

        Assert.False(refreshed.IsSuccess);
    }

    [Fact]
    public async Task PasswordGrant_WrongPassword_IsRejected()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityplayer");

        OidcTokenResponse response = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            EMAIL,
            "wrong-password",
            SCOPE);

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("invalid_grant", response.Error);
    }

    [Fact]
    public async Task PasswordGrant_UnknownUser_IsRejected()
    {
        await SeedPlatformConfigAsync();

        OidcTokenResponse response = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.UNITY_CLIENT_ID,
            "nobody@test.com",
            PASSWORD,
            SCOPE);

        Assert.False(response.IsSuccess);
        Assert.Equal("invalid_grant", response.Error);
    }

    [Fact]
    public async Task PasswordGrant_WebClient_IsRejected()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, "unityplayer");

        // У веб-клиента нет permission grant_types:password.
        OidcTokenResponse response = await _oidc.ExecutePasswordFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            EMAIL,
            PASSWORD,
            SCOPE);

        Assert.False(response.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
