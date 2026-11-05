using CSharpFunctionalExtensions;
using RealtimeService.Contracts.Dtos;
using RealtimeService.Core.Abstractions;
using RealtimeService.Core.Configuration;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;
using Microsoft.Extensions.Options;

namespace RealtimeService.Core.Features.Realtime.Commands.IssueTicket;

public sealed record IssueTicketCommand(Guid AccountId) : ICommand;

public sealed class IssueTicketHandler(
    ITicketStore ticketStore,
    IOptions<RealtimeOptions> options) : ICommandHandler<RealtimeTicketDto, IssueTicketCommand>
{
    public async Task<Result<RealtimeTicketDto, Error>> Handle(
        IssueTicketCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.AccountId == Guid.Empty)
        {
            return GeneralErrors.ValueIsRequired("accountId");
        }

        string ticket = await ticketStore.IssueAsync(command.AccountId, cancellationToken);

        return new RealtimeTicketDto(ticket, options.Value.TicketTtlSeconds);
    }
}
