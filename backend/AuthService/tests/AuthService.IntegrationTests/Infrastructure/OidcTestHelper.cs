using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AuthService.IntegrationTests.Infrastructure;

public sealed class OidcTestHelper
{
    private readonly IntegrationTestsWebFactory _factory;
    private readonly HttpClient _browser;

    public OidcTestHelper(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
        _browser = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
    }

    public async Task LoginAsync(string email, string password)
    {
        HttpResponseMessage response = await _browser.PostAsJsonAsync("/auth/login", new { email, password });
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException(
                $"Login failed: {response.StatusCode} — {await response.Content.ReadAsStringAsync()}");
        }
    }

    public Task<HttpResponseMessage> AuthorizeAsync(
        string clientId,
        string scope,
        string redirectUri,
        string? codeChallenge)
    {
        var query = new List<string>
        {
            "response_type=code",
            $"client_id={Uri.EscapeDataString(clientId)}",
            $"redirect_uri={Uri.EscapeDataString(redirectUri)}",
            $"scope={Uri.EscapeDataString(scope)}",
        };

        if (codeChallenge is not null)
        {
            query.Add($"code_challenge={codeChallenge}");
            query.Add("code_challenge_method=S256");
        }

        return _browser.GetAsync($"/connect/authorize?{string.Join('&', query)}");
    }

    public async Task<OidcTokenResponse> ExecuteAuthorizationCodeFlowAsync(
        string clientId,
        string? clientSecret,
        string scope,
        string redirectUri = IntegrationTestsWebFactory.WEB_REDIRECT_URI)
    {
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        HttpResponseMessage authorizeResponse = await AuthorizeAsync(clientId, scope, redirectUri, codeChallenge);

        if (authorizeResponse.StatusCode is not (HttpStatusCode.Redirect or HttpStatusCode.Found))
        {
            throw new InvalidOperationException(
                $"Expected redirect from /connect/authorize, got {authorizeResponse.StatusCode}: "
                + await authorizeResponse.Content.ReadAsStringAsync());
        }

        Uri location = authorizeResponse.Headers.Location
            ?? throw new InvalidOperationException("Redirect response has no Location header.");

        string? error = ExtractQueryParameter(location, "error");
        if (error is not null)
        {
            throw new InvalidOperationException($"Authorization failed: {error}");
        }

        string code = ExtractQueryParameter(location, "code")
            ?? throw new InvalidOperationException($"No authorization code in redirect: {location}");

        return await ExchangeCodeAsync(clientId, clientSecret, code, redirectUri, codeVerifier);
    }

    public Task<OidcTokenResponse> ExchangeCodeAsync(
        string clientId,
        string? clientSecret,
        string code,
        string redirectUri,
        string codeVerifier)
    {
        var body = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "authorization_code"),
            new("client_id", clientId),
            new("code", code),
            new("redirect_uri", redirectUri),
            new("code_verifier", codeVerifier),
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            body.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
        }

        return PostTokenAsync(body);
    }

    public Task<OidcTokenResponse> ExecuteClientCredentialsFlowAsync(
        string clientId,
        string clientSecret,
        string scope) =>
        PostTokenAsync(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("scope", scope),
        ]);

    // In-game логин Unity-клиента: email/username + пароль напрямую в /connect/token.
    public Task<OidcTokenResponse> ExecutePasswordFlowAsync(
        string clientId,
        string username,
        string password,
        string scope) =>
        PostTokenAsync(
        [
            new KeyValuePair<string, string>("grant_type", "password"),
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("password", password),
            new KeyValuePair<string, string>("scope", scope),
        ]);

    public Task<OidcTokenResponse> RefreshTokenAsync(
        string clientId,
        string? clientSecret,
        string refreshToken)
    {
        var body = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "refresh_token"),
            new("client_id", clientId),
            new("refresh_token", refreshToken),
        };

        if (!string.IsNullOrEmpty(clientSecret))
        {
            body.Add(new KeyValuePair<string, string>("client_secret", clientSecret));
        }

        return PostTokenAsync(body);
    }

    public async Task<HttpResponseMessage> RevokeTokenAsync(string clientId, string token, string tokenTypeHint)
    {
        using HttpClient client = _factory.CreateClient();

        FormUrlEncodedContent body = new(
        [
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("token", token),
            new KeyValuePair<string, string>("token_type_hint", tokenTypeHint),
        ]);

        return await client.PostAsync("/connect/revoke", body);
    }

    public async Task<HttpResponseMessage> GetUserInfoAsync(string accessToken)
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await client.GetAsync("/connect/userinfo");
    }

    public static JwtClaims ParseJwt(string token) => JwtClaims.Parse(token);

    public static string? GetQueryParameter(Uri uri, string name) => ExtractQueryParameter(uri, name);

    public static string GenerateCodeVerifier()
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(32);
        return Base64Url(bytes);
    }

    public static string GenerateCodeChallenge(string codeVerifier)
    {
        byte[] hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64Url(hash);
    }

    private async Task<OidcTokenResponse> PostTokenAsync(IEnumerable<KeyValuePair<string, string>> body)
    {
        using HttpClient client = _factory.CreateClient();
        using FormUrlEncodedContent content = new(body);
        using HttpResponseMessage response = await client.PostAsync("/connect/token", content);

        return await OidcTokenResponse.ParseAsync(response);
    }

    private static string? ExtractQueryParameter(Uri uri, string name)
    {
        foreach (string part in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = part.Split('=', 2);
            if (keyValue.Length == 2 && string.Equals(keyValue[0], name, StringComparison.Ordinal))
            {
                return Uri.UnescapeDataString(keyValue[1]);
            }
        }

        return null;
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}

public sealed record OidcTokenResponse
{
    public bool IsSuccess { get; private init; }

    public HttpStatusCode StatusCode { get; private init; }

    public string? AccessToken { get; private init; }

    public string? RefreshToken { get; private init; }

    public string? IdToken { get; private init; }

    public string? TokenType { get; private init; }

    public int ExpiresIn { get; private init; }

    public string? Scope { get; private init; }

    public string? Error { get; private init; }

    public string? RawResponse { get; private init; }

    internal static async Task<OidcTokenResponse> ParseAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();

        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        return new OidcTokenResponse
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = response.StatusCode,
            AccessToken = GetString(root, "access_token"),
            RefreshToken = GetString(root, "refresh_token"),
            IdToken = GetString(root, "id_token"),
            TokenType = GetString(root, "token_type"),
            ExpiresIn = GetInt32(root, "expires_in"),
            Scope = GetString(root, "scope"),
            Error = GetString(root, "error"),
            RawResponse = json,
        };
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static int GetInt32(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out JsonElement value) && value.TryGetInt32(out int result)
            ? result
            : 0;
}

public sealed class JwtClaims
{
    private readonly IReadOnlyDictionary<string, JsonElement> _claims;

    private JwtClaims(IReadOnlyDictionary<string, JsonElement> claims) => _claims = claims;

    public static JwtClaims Parse(string token)
    {
        string[] parts = token.Split('.');
        if (parts.Length < 2)
        {
            throw new InvalidOperationException("Invalid JWT format.");
        }

        string payload = parts[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + ((4 - (payload.Length % 4)) % 4), '=');

        byte[] bytes = Convert.FromBase64String(payload);

        using JsonDocument document = JsonDocument.Parse(bytes);

        return new JwtClaims(document.RootElement
            .EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.Clone(), StringComparer.Ordinal));
    }

    public bool HasClaim(string name) => _claims.ContainsKey(name);

    public string? GetString(string name) =>
        _claims.TryGetValue(name, out JsonElement value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    public IReadOnlyList<string> GetValues(string name)
    {
        if (!_claims.TryGetValue(name, out JsonElement value))
        {
            return [];
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => [value.GetString()!],
            JsonValueKind.Array => value.EnumerateArray()
                .Where(item => item.ValueKind == JsonValueKind.String)
                .Select(item => item.GetString()!)
                .ToArray(),
            _ => [],
        };
    }
}
