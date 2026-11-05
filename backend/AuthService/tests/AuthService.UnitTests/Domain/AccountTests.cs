using AuthService.Domain.Accounts;
using CSharpFunctionalExtensions;

namespace AuthService.UnitTests.Domain;

public class AccountTests
{
    private const string EMAIL = "user@test.com";
    private const string USER_NAME = "testuser";

    [Fact]
    public void Create_WithValidData_Succeeds()
    {
        Result<Account, Shared.SharedKernel.Errors.Error> result = Account.Create(USER_NAME, EMAIL);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(USER_NAME, result.Value.UserName);
        Assert.Equal(EMAIL, result.Value.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUserName_Fails(string userName)
    {
        Result<Account, Shared.SharedKernel.Errors.Error> result = Account.Create(userName, EMAIL);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyEmail_Fails(string email)
    {
        Result<Account, Shared.SharedKernel.Errors.Error> result = Account.Create(USER_NAME, email);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithTooLongUserName_Fails()
    {
        string userName = new('a', AccountConstants.USER_NAME_MAX_LENGTH + 1);

        Result<Account, Shared.SharedKernel.Errors.Error> result = Account.Create(userName, EMAIL);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithTooLongEmail_Fails()
    {
        string email = new('a', AccountConstants.EMAIL_MAX_LENGTH + 1);

        Result<Account, Shared.SharedKernel.Errors.Error> result = Account.Create(USER_NAME, email);

        Assert.True(result.IsFailure);
    }
}
