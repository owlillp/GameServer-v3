using ProfileService.Domain.Players;

namespace ProfileService.Core.Abstractions;

public interface IReadDbContext
{
    IQueryable<PlayerProfile> ProfilesRead { get; }
}
