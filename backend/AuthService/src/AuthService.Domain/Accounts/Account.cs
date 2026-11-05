using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Shared.SharedKernel.Errors;

namespace AuthService.Domain.Accounts;

public sealed class Account : IdentityUser<Guid>
{
    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core
    private Account() { }

    private Account(string email, string userName)
    {
        Id = Guid.CreateVersion7();
        Email = email;
        UserName = userName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<Account, Error> Create(string userName, string email)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            return GeneralErrors.ValueIsRequired("userName");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return GeneralErrors.ValueIsRequired("email");
        }

        if (userName.Length > AccountConstants.USER_NAME_MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("userName");
        }

        if (email.Length > AccountConstants.EMAIL_MAX_LENGTH)
        {
            return GeneralErrors.ValueIsInvalid("email");
        }

        return new Account(email, userName);
    }
}
