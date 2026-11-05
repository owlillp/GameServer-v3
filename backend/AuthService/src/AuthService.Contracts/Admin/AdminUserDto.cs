namespace AuthService.Contracts.Admin;

public sealed record AdminUserDto(
    Guid Id,
    string? Email,
    string? UserName,
    IReadOnlyList<string> Roles,
    bool IsLockedOut,
    DateTime CreatedAt);
