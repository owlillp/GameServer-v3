namespace AuthService.Core.Configuration;

public sealed class IdentityOptions
{
    public const string SECTION_NAME = "Identity";

    public PasswordOptions Password { get; set; } = new();

    public LockoutOptions Lockout { get; set; } = new();

    public UserOptions User { get; set; } = new();

    public SignInOptions SignIn { get; set; } = new();
}

public sealed class PasswordOptions
{
    public int RequiredLength { get; set; } = 8;

    public bool RequireDigit { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireNonAlphanumeric { get; set; }
}

public sealed class LockoutOptions
{
    public int MaxFailedAccessAttempts { get; set; } = 5;

    public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
}

public sealed class UserOptions
{
    public bool RequireUniqueEmail { get; set; } = true;
}

public sealed class SignInOptions
{
    public bool RequireConfirmedEmail { get; set; }
}