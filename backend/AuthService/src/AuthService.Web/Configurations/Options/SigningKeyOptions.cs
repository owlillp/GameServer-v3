namespace AuthService.Web.Configurations.Options;

public sealed class SigningKeyOptions
{
    public const string SECTION_NAME = "SigningKeys";

    public string? SigningKeyBase64 { get; init; }

    public string? EncryptionKeyBase64 { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(SigningKeyBase64)
        && !string.IsNullOrWhiteSpace(EncryptionKeyBase64);
}