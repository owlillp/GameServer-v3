namespace AuthService.Contracts.Responses;

public sealed record LoginResponse(Guid AccountId, string Email, string UserName);