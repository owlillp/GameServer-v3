using AuthService.Domain.Authorization;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Connect;

public sealed class AuthorizationCodeFlowTests : IntegrationTestsBase
{
    private const string EMAIL = "web-user@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string USER_NAME = "webuser";
    private const string SCOPE = "openid profile email offline_access auth";

    private readonly OidcTestHelper _oidc;

    public AuthorizationCodeFlowTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _oidc = new OidcTestHelper(factory);
    }

    [Fact]
    public async Task AuthorizationCode_WithPkce_ReturnsTokensWithUserClaims()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);
        await _oidc.LoginAsync(EMAIL, PASSWORD);

        OidcTokenResponse response = await _oidc.ExecuteAuthorizationCodeFlowAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            clientSecret: null,
            SCOPE);

        Assert.True(response.IsSuccess, response.RawResponse);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.IdToken);
        Assert.Equal("Bearer", response.TokenType);

        JwtClaims claims = OidcTestHelper.ParseJwt(response.AccessToken);
        Assert.Equal(EMAIL, claims.GetString("email"));
        Assert.Contains(AuthRoles.USER, claims.GetValues("role"), StringComparer.Ordinal);
    }

    [Fact]
    public async Task Authorize_WithoutLogin_RedirectsToFrontendLogin()
    {
        await SeedPlatformConfigAsync();

        string codeVerifier = OidcTestHelper.GenerateCodeVerifier();
        string codeChallenge = OidcTestHelper.GenerateCodeChallenge(codeVerifier);

        HttpResponseMessage response = await _oidc.AuthorizeAsync(
            IntegrationTestsWebFactory.WEB_CLIENT_ID,
            SCOPE,
            IntegrationTestsWebFactory.WEB_REDIRECT_URI,
            codeChallenge);

        Assert.NotNull(response.Headers.Location);
        Assert.StartsWith(IntegrationTestsWebFactory.FRONTEND_LOGIN_URL, response.Headers.Location.ToString(), StringComparison.Ordinal);
    }
}
