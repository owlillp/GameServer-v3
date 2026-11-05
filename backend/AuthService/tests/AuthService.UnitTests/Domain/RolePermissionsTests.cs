using AuthService.Domain.Authorization;

namespace AuthService.UnitTests.Domain;

public class RolePermissionsTests
{
    [Fact]
    public void ForRole_Admin_HasAllPermissions()
    {
        IReadOnlyList<string> permissions = RolePermissions.ForRole(AuthRoles.ADMIN);

        Assert.Equal(AuthPermissions.All.Order(StringComparer.Ordinal), permissions.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void ForRole_Moderator_HasUsersAndPlatformPermissions()
    {
        IReadOnlyList<string> permissions = RolePermissions.ForRole(AuthRoles.MODERATOR);

        Assert.Contains(AuthPermissions.Users.VIEW, permissions, StringComparer.Ordinal);
        Assert.Contains(AuthPermissions.Users.MANAGE, permissions, StringComparer.Ordinal);
        Assert.Contains(AuthPermissions.Platform.MANAGE, permissions, StringComparer.Ordinal);
    }

    [Theory]
    [InlineData(AuthRoles.USER)]
    [InlineData(AuthRoles.SERVICE)]
    public void ForRole_WithoutPrivileges_IsEmpty(string role)
    {
        Assert.Empty(RolePermissions.ForRole(role));
    }

    [Fact]
    public void ForRole_UnknownRole_IsEmpty()
    {
        Assert.Empty(RolePermissions.ForRole("Unknown"));
    }

    [Fact]
    public void ForRoles_UnionsPermissionsWithoutDuplicates()
    {
        IReadOnlyCollection<string> permissions = RolePermissions.ForRoles([AuthRoles.USER, AuthRoles.ADMIN]);

        Assert.Equal(AuthPermissions.All.Order(StringComparer.Ordinal), permissions.Order(StringComparer.Ordinal));
    }
}
