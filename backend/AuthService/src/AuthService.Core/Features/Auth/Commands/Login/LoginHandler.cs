using System.Globalization;
using System.Security.Claims;
using AuthService.Contracts.Responses;
using AuthService.Core.Extensions;
using AuthService.Domain.Accounts;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Authentication.Authentication;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.Login;

public sealed class LoginHandler(
    IValidator<LoginCommand> validator,
    SignInManager<Account> signInManager,
    UserManager<Account> userManager,
    ITransactionManager transactionManager)
    : ICommandHandler<LoginResponse, LoginCommand>
{
    public async Task<Result<LoginResponse, Error>> Handle(
        LoginCommand command,
        CancellationToken cancellationToken = new ())
    {
        var validationResult = await validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToError();
        }

        var transactionResult = await transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionResult.IsFailure)
        {
            return transactionResult.Error;
        }

        var account = await userManager.FindByEmailAsync(command.Request.Email);
        if (account is null)
        {
            return InvalidCredentials();
        }

        var checkResult = await signInManager.CheckPasswordSignInAsync(
            account,
            command.Request.Password,
            lockoutOnFailure: true);

        if (!checkResult.Succeeded)
        {
            return MapSignInFailure(checkResult);
        }

        var claims = new List<Claim>
        {
            new(AuthClaimTypes.SUB, account.Id.ToString()),
            new(AuthClaimTypes.NAME, account.UserName!),
            new(AuthClaimTypes.EMAIL, account.Email!),
            new(AuthClaimTypes.SECURITY_STAMP, account.SecurityStamp!),
            new(AuthClaimTypes.AUTH_METHOD, "pwd"),
            new(AuthClaimTypes.ACCOUNT_CREATED_AT, account.CreatedAt.ToString("O", CultureInfo.InvariantCulture)),
        };

        await signInManager.SignInWithClaimsAsync(claims, isPersistent: true);

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return new LoginResponse(account.Id, account.Email!, account.UserName!);
    }

    private static Error InvalidCredentials() =>
        Error.Authentication("invalid.credentials", "Invalid email or password");

    private static Error MapSignInFailure(SignInResult result)
    {
        if (result.IsLockedOut)
        {
            return Error.Authentication(
                "account.locked",
                "Аккаунт временно заблокирован из-за множества неудачных попыток входа");
        }

        if (result.IsNotAllowed)
        {
            return Error.Authentication(
                "account.not.allowed",
                "Вход не разрешён (требуется подтверждение email/телефона)");
        }

        if (result.RequiresTwoFactor)
        {
            return Error.Authentication(
                "two.factor.required",
                "Требуется двухфакторная аутентификация");
        }

        return InvalidCredentials();
    }
}