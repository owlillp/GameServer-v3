using AuthService.Contracts.Requests;
using AuthService.Core.Features.Auth.Commands.Login;
using FluentValidation.Results;

namespace AuthService.UnitTests.Features.Auth;

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        LoginCommand command = new(new LoginRequest("user@test.com", "StrongPass1!"));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithInvalidEmail_IsInvalid()
    {
        LoginCommand command = new(new LoginRequest("not-an-email", "StrongPass1!"));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith("Request.Email", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_IsInvalid()
    {
        LoginCommand command = new(new LoginRequest("user@test.com", ""));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith("Request.Password", StringComparison.Ordinal));
    }
}
