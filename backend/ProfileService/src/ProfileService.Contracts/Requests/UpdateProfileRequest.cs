namespace ProfileService.Contracts.Requests;

public sealed record UpdateProfileRequest(string Nickname, string? Region);
