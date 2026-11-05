using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using ProfileService.Domain.Players;
using Shared.SharedKernel.Errors;

namespace ProfileService.Core.Abstractions;

public interface IProfileRepository
{
    Task<Result<Guid, Error>> Add(PlayerProfile profile, CancellationToken cancellationToken);

    Task<Result<PlayerProfile, Error>> GetByAsync(Expression<Func<PlayerProfile, bool>> expression, CancellationToken cancellationToken);
}