using AuthService.Contracts.Requests;
using AuthService.Core.Features.Auth.Commands.Register;
using FluentValidation.Results;

namespace AuthService.UnitTests.Features.Auth;

public class RegisterValidatorTests
{
    private readonly RegisterValidator _validator = new();

    [Fact]
    public async Task Validate_WithValidRequest_IsValid()
    {
        RegisterCommand command = new(new RegisterRequest("user@test.com", "testuser", "StrongPass1!"));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public async Task Validate_WithInvalidEmail_IsInvalid(string email)
    {
        RegisterCommand command = new(new RegisterRequest(email, "testuser", "StrongPass1!"));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith("Request.Email", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Validate_WithEmptyUserName_IsInvalid()
    {
        RegisterCommand command = new(new RegisterRequest("user@test.com", "", "StrongPass1!"));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith("Request.UserName", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Validate_WithEmptyPassword_IsInvalid()
    {
        RegisterCommand command = new(new RegisterRequest("user@test.com", "testuser", ""));

        ValidationResult result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith("Request.Password", StringComparison.Ordinal));
    }
}
