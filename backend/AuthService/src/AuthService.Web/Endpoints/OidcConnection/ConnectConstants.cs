namespace AuthService.Web.Endpoints.OidcConnection;

public static class ConnectConstants
{
    public const string AUTHORIZE_ENDPOINT = "connect/authorize";
    public const string TOKEN_ENDPOINT = "connect/token";
    public const string REVOKE_ENDPOINT = "connect/revoke";
    public const string USER_INFO_ENDPOINT = "connect/userinfo";
    public const string END_SESSION_ENDPOINT = "connect/logout";
    public const string TOKEN_RATE_LIMIT_POLICY = "token";
}