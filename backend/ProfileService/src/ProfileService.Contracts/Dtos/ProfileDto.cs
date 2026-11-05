namespace ProfileService.Contracts.Dtos;

public sealed record ProfileDto(
    Guid Id,
    string Nickname,
    string? Region,
    string? Email,
    string? UserName,
    DateTime CreatedAt,
    DateTime UpdatedAt);
