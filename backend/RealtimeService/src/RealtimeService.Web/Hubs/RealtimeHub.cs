using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RealtimeService.Core.Abstractions;
using Shared.IntegrationEvents.Realtime;

namespace RealtimeService.Web.Hubs;

[AllowAnonymous]
public sealed class RealtimeHub(
    ITicketStore ticketStore,
    ILogger<RealtimeHub> logger) : Hub
{
    private const string ACCOUNT_ID_ITEM = "realtime:accountId";

    public async Task Authenticate(string ticket)
    {
        if (Context.Items.ContainsKey(ACCOUNT_ID_ITEM))
        {
            throw new HubException("realtime.auth.already_authenticated");
        }

        Guid? accountId = await ticketStore.ConsumeAsync(ticket, Context.ConnectionAborted);
        if (accountId is null)
        {
            logger.LogWarning(
                "Realtime connection {ConnectionId} presented an invalid ticket",
                Context.ConnectionId);

            throw new HubException("realtime.auth.invalid_ticket");
        }

        Context.Items[ACCOUNT_ID_ITEM] = accountId.Value;

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeAudiences.LIVE_OPS,
            Context.ConnectionAborted);

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            RealtimeAudiences.Player(accountId.Value),
            Context.ConnectionAborted);

        await Clients.Caller.SendAsync("Authenticated", accountId.Value, Context.ConnectionAborted);

        logger.LogInformation(
            "Realtime connection {ConnectionId} authenticated for account {AccountId}",
            Context.ConnectionId,
            accountId.Value);
    }

}
