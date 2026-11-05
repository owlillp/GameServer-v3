using AuthService.Contracts.Admin;
using AuthService.Core.Abstractions;
using AuthService.Domain.Authorization;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;

namespace AuthService.Core.Features.Admin.Queries.GetStats;

public sealed class GetStatsHandler(IReadDbContext readDbContext)
    : IQueryHandler<AdminStatsDto, GetStatsQuery>
{
    public async Task<Result<AdminStatsDto, Error>> Handle(
        GetStatsQuery query,
        CancellationToken cancellationToken = new ())
    {
        int totalUsers = await readDbContext.AccountsRead.CountAsync(cancellationToken);

        var roleCounts = await (
                from accountRole in readDbContext.AccountRolesRead
                join role in readDbContext.RolesRead on accountRole.RoleId equals role.Id
                group accountRole by role.Name
                into roleGroup
                select new { RoleName = roleGroup.Key, Count = roleGroup.Count() })
            .ToDictionaryAsync(
                row => row.RoleName!,
                row => row.Count,
                StringComparer.Ordinal,
                cancellationToken);

        var now = DateTimeOffset.UtcNow;
        int lockedOutCount = await readDbContext.AccountsRead
            .CountAsync(
                account => account.LockoutEnd != null && account.LockoutEnd > now,
                cancellationToken);

        return new AdminStatsDto(
            totalUsers,
            roleCounts.GetValueOrDefault(AuthRoles.ADMIN),
            roleCounts.GetValueOrDefault(AuthRoles.MODERATOR),
            lockedOutCount);
    }
}
