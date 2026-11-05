using System.Net;
using System.Net.Http.Json;
using AuthService.Contracts.Requests;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features.Auth;

public sealed class AuthEndpointsTests : IntegrationTestsBase
{
    private const string EMAIL = "auth-endpoints@test.com";
    private const string PASSWORD = "TestPass123!";
    private const string USER_NAME = "authendpoints";

    public AuthEndpointsTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Register_WithValidRequest_CreatesAccount()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(EMAIL, USER_NAME, PASSWORD));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Guid accountId = await GetAccountIdAsync(EMAIL);
        Assert.NotEqual(Guid.Empty, accountId);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest(EMAIL, "anotheruser", PASSWORD));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ReturnsBadRequest()
    {
        await SeedPlatformConfigAsync();

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/register",
            new RegisterRequest("not-an-email", USER_NAME, PASSWORD));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccess()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(EMAIL, PASSWORD));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        await SeedPlatformConfigAsync();
        await RegisterAsync(EMAIL, PASSWORD, USER_NAME);

        HttpResponseMessage response = await HttpClient.PostAsJsonAsync(
            "/auth/login",
            new LoginRequest(EMAIL, "wrong-password"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
