using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProfileService.Core.Abstractions;
using ProfileService.Domain.Players;
using Shared.SharedKernel.Errors;

namespace ProfileService.Infrastructure.Postgres.Repositories;

public sealed class ProfileRepository(
    ILogger<ProfileRepository> logger,
    ProfileServiceDbContext dbContext) : IProfileRepository
{
    public Task<Result<Guid, Error>> Add(PlayerProfile profile, CancellationToken cancellationToken)
    {
        try
        {
            dbContext.Profiles.Add(profile);
            return Task.FromResult<Result<Guid, Error>>(profile.Id);
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was canceled while creating profile");
            return Task.FromResult<Result<Guid, Error>>(GeneralErrors.Failure("Process create profile"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while creating profile");
            return Task.FromResult<Result<Guid, Error>>(GeneralErrors.Failure());
        }
    }

    public async Task<Result<PlayerProfile, Error>> GetByAsync(
        Expression<Func<PlayerProfile, bool>> expression,
        CancellationToken cancellationToken)
    {
        try
        {
            var profile = await dbContext.Profiles
                .FirstOrDefaultAsync(expression, cancellationToken);

            return profile != null
                ? profile
                : GeneralErrors.NotFound();
        }
        catch (OperationCanceledException ex)
        {
            logger.LogError(ex, "Operation was cancelled while getting profile");
            return GeneralErrors.Failure("Process get profile");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while getting profile");
            return GeneralErrors.Failure();
        }
    }
}