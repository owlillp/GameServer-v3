using FluentValidation;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.Login;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(c => c.Request.Email)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("email"))
            .EmailAddress().WithError(GeneralErrors.ValueIsInvalid("email"));

        RuleFor(c => c.Request.Password)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("password"));
    }
}