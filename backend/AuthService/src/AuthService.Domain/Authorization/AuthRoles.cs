namespace AuthService.Domain.Authorization;

public static class AuthRoles
{
    public const string USER = "User";
    public const string MODERATOR = "Moderator";
    public const string ADMIN = "Admin";

    public const string SERVICE = "Service";

    public static IReadOnlyList<string> All { get; } = [USER, MODERATOR, ADMIN];
    public static IReadOnlyList<string> Assignable { get; } = [MODERATOR, ADMIN];

    public static bool IsKnownRole(string roleName)
        => All.Contains(roleName, StringComparer.Ordinal);
}