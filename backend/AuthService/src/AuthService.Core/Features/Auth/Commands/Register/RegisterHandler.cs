using AuthService.Contracts.Responses;
using AuthService.Core.Messaging.Publishers;
using AuthService.Domain.Accounts;
using AuthService.Domain.Authorization;
using CSharpFunctionalExtensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Shared.Core.Abstractions;
using Shared.Core.Database;
using Shared.Core.Validation;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Auth.Commands.Register;

public sealed class RegisterHandler(
    IValidator<RegisterCommand> validator,
    UserManager<Account> userManager,
    IAccountRegisteredEventPublisher eventPublisher,
    ITransactionManager transactionManager)
    : ICommandHandler<RegisterResponse, RegisterCommand>
{
    public async Task<Result<RegisterResponse, Error>> Handle(
        RegisterCommand command,
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

        var accountResult = Account.Create(command.Request.UserName, command.Request.Email);
        if (accountResult.IsFailure)
        {
            return accountResult.Error;
        }

        var account = accountResult.Value;
        var result = await userManager.CreateAsync(account, command.Request.Password);
        if (!result.Succeeded)
        {
            return MapIdentityErrors(result);
        }

        var roleResult = await userManager.AddToRoleAsync(account, AuthRoles.USER);
        if (!roleResult.Succeeded)
        {
            return MapIdentityErrors(roleResult);
        }

        await eventPublisher.PublishAsync(account.Id, account.UserName!);

        var commitResult = await transactionManager.CommitTransactionAsync(cancellationToken);
        if (commitResult.IsFailure)
        {
            return commitResult.Error;
        }

        return new RegisterResponse(account.Id);
    }

    private static Error MapIdentityErrors(IdentityResult result)
    {
        var messages = result.Errors
            .Select(e => new ErrorMessage(e.Code, e.Description))
            .ToArray();

        bool isDuplicate = result.Errors.Any(e =>
            e.Code.StartsWith("Duplicate", StringComparison.Ordinal));

        return isDuplicate
            ? Error.Conflict(messages)
            : Error.Validation(messages);
    }
}