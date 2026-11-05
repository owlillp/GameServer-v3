using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace ProfileService.Domain.Players;

public sealed record Region
{
    public string Value { get; init; }

    private Region(string value) => Value = value;

    public static Result<Region, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return GeneralErrors.ValueIsRequired("nickname");
        }

        if (value.Length > PlayerProfileConstants.REGION_MAX_LENGTH)
        {
            return GeneralErrors.LengthIsInvalid("region", max: PlayerProfileConstants.REGION_MAX_LENGTH);
        }

        return new Region(value);
    }
}