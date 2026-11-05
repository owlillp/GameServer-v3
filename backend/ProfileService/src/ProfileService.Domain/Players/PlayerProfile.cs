using CSharpFunctionalExtensions;
using Shared.SharedKernel.Errors;

namespace ProfileService.Domain.Players;

public sealed class PlayerProfile
{
    public Guid Id { get; private set; }

    public Nickname Nickname { get; private set; } = null!;

    public Region? Region { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    // EF Core
    private PlayerProfile() { }

    private PlayerProfile(Guid accountId, Nickname nickname, Region? region)
    {
        Id = accountId;
        Nickname = nickname;
        Region = region;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public static Result<PlayerProfile, Error> Create(Guid accountId, Nickname nickname, Region? region = null)
    {
        if (accountId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("accountId");
        }

        return new PlayerProfile(accountId, nickname, region);
    }

    public void UpdateNickname(Nickname nickname)
    {
        Nickname = nickname;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRegion(Region? region)
    {
        Region = region;
        UpdatedAt = DateTime.UtcNow;
    }
}
