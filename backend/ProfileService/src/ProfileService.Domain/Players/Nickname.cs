using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace ProfileService.Domain.Players;

public sealed record Nickname
{
    public string Value { get; init; }

    private Nickname(string value) => Value = value;

    public static Result<Nickname, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("nickname");
        }

        if (value.Length > PlayerProfileConstants.NICKNAME_MAX_LENGTH)
        {
            return GeneralErrors.LengthIsInvalid("nickname", max: PlayerProfileConstants.NICKNAME_MAX_LENGTH);
        }

        return new Nickname(value);
    }
}