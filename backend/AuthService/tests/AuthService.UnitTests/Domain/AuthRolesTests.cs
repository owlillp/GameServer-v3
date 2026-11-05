using AuthService.Domain.Authorization;

namespace AuthService.UnitTests.Domain;

public class AuthRolesTests
{
    [Theory]
    [InlineData(AuthRoles.USER)]
    [InlineData(AuthRoles.MODERATOR)]
    [InlineData(AuthRoles.ADMIN)]
    public void IsKnownRole_WithKnownRole_ReturnsTrue(string role)
    {
        Assert.True(AuthRoles.IsKnownRole(role));
    }

    [Theory]
    [InlineData(AuthRoles.SERVICE)]
    [InlineData("Unknown")]
    [InlineData("")]
    public void IsKnownRole_WithUnknownRole_ReturnsFalse(string role)
    {
        Assert.False(AuthRoles.IsKnownRole(role));
    }

    [Fact]
    public void All_ContainsUserModeratorAdmin()
    {
        Assert.Equal(new[] { AuthRoles.USER, AuthRoles.MODERATOR, AuthRoles.ADMIN }, AuthRoles.All);
    }

    [Fact]
    public void Assignable_ContainsModeratorAndAdminOnly()
    {
        Assert.Equal(new[] { AuthRoles.MODERATOR, AuthRoles.ADMIN }, AuthRoles.Assignable);
    }
}
