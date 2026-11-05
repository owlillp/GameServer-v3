namespace AuthService.Contracts.Requests;

public sealed record RegisterRequest(string Email, string UserName, string Password);