using Microsoft.Extensions.Logging;
using ProfileService.Core.Abstractions;
using ProfileService.Domain.Players;
using Shared.Core.Database;
using Shared.IntegrationEvents.Identity.Events;

namespace ProfileService.Core.Messaging.Handlers;

public sealed class AccountRegisteredEventHandler(
    ILogger<AccountRegisteredEventHandler> logger,
    IProfileRepository profileRepository,
    ITransactionManager transactionManager)
{
    public async Task Handle(AccountRegisteredEvent @event, CancellationToken cancellationToken)
    {
        logger.LogInformation("Received AccountRegisteredEvent for account: {AccountId}", @event.AccountId);

        var nicknameResult = Nickname.Create(@event.Username);
        if (nicknameResult.IsFailure)
        {
            logger.LogError(
                "Failed to create profile: {AccountId}, error: {Error}",
                @event.AccountId,
                nicknameResult.Error);

            return;
        }

        var profileResult = PlayerProfile.Create(@event.AccountId, nicknameResult.Value);
        if (profileResult.IsFailure)
        {
            logger.LogError(
                "Failed to create profile: {AccountId}, error: {Error}",
                @event.AccountId,
                profileResult.Error);

            return;
        }

        var addResult = await profileRepository.Add(profileResult.Value, cancellationToken);
        if (addResult.IsFailure)
        {
            logger.LogError(
                "Failed to create profile: {AccountId}, error: {Error}",
                @event.AccountId,
                addResult.Error);

            return;
        }

        var saveChangesResult = await transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
        {
            logger.LogError(
                "Failed to create profile: {AccountId}, error: {Error}",
                @event.AccountId,
                saveChangesResult.Error);

            return;
        }

        logger.LogInformation("Profile created: {ProfileId}", profileResult.Value.Id);
    }
}