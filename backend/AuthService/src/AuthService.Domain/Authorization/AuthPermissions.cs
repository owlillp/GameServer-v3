namespace AuthService.Domain.Authorization;

public static class AuthPermissions
{
    public static class Users
    {
        public const string VIEW = "users.view";
        public const string MANAGE = "users.manage";
    }

    public static class Platform
    {
        public const string MANAGE = "platform.manage";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Users.VIEW,
        Users.MANAGE,
        Platform.MANAGE
    ];
}