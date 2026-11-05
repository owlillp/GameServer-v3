using AuthService.Domain.Accounts;
using FluentValidation;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.Register;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(c => c.Request.Email)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("email"))
            .EmailAddress().WithError(GeneralErrors.ValueIsInvalid("email"))
            .MaximumLength(AccountConstants.EMAIL_MAX_LENGTH)
            .WithError(GeneralErrors.LengthIsInvalid("email", max: AccountConstants.EMAIL_MAX_LENGTH));

        RuleFor(c => c.Request.UserName)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("userName"))
            .MaximumLength(AccountConstants.USER_NAME_MAX_LENGTH)
            .WithError(GeneralErrors.LengthIsInvalid("userName", max: AccountConstants.USER_NAME_MAX_LENGTH));

        RuleFor(c => c.Request.Password)
            .NotEmpty().WithError(GeneralErrors.ValueIsRequired("password"));
    }
}