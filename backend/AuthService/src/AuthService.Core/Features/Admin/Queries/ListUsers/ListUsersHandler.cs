using AuthService.Contracts.Admin;
using AuthService.Core.Abstractions;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Abstractions;
using Shared.SharedKernel.Errors;
using Shared.SharedKernel.Responses;

namespace AuthService.Core.Features.Admin.Queries.ListUsers;

public sealed class ListUsersHandler(IReadDbContext readDbContext)
    : IQueryHandler<PaginationResponse<AdminUserDto>, ListUsersQuery>
{
    private const int DEFAULT_PAGE_SIZE = 20;
    private const int MAX_PAGE_SIZE = 100;

    public async Task<Result<PaginationResponse<AdminUserDto>, Error>> Handle(
        ListUsersQuery query,
        CancellationToken cancellationToken = new ())
    {
        int page = Math.Max(1, query.Page);
        int pageSize = query.PageSize <= 0
            ? DEFAULT_PAGE_SIZE
            : Math.Min(query.PageSize, MAX_PAGE_SIZE);

        var accountsQuery = readDbContext.AccountsRead;

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string pattern = $"%{query.Search.Trim().ToUpperInvariant()}%";

            accountsQuery = accountsQuery.Where(account =>
                (account.NormalizedEmail != null && EF.Functions.Like(account.NormalizedEmail, pattern))
                || (account.NormalizedUserName != null && EF.Functions.Like(account.NormalizedUserName, pattern)));
        }

        int totalCount = await accountsQuery.CountAsync(cancellationToken);

        var accounts = await accountsQuery
            .OrderByDescending(account => account.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var rolesByUser = await GetRolesByUserAsync(
            accounts.Select(account => account.Id).ToArray(),
            cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var users = accounts
            .ConvertAll(account => new AdminUserDto(
                account.Id,
                account.Email,
                account.UserName,
                rolesByUser.TryGetValue(account.Id, out List<string>? roles) ? roles : [],
                account.LockoutEnd is not null && account.LockoutEnd > now,
                account.CreatedAt))
;

        int totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        return new PaginationResponse<AdminUserDto>(users, totalCount, page, pageSize, totalPages);
    }

    private async Task<Dictionary<Guid, List<string>>> GetRolesByUserAsync(
        Guid[] userIds,
        CancellationToken cancellationToken)
    {
        if (userIds.Length == 0)
        {
            return [];
        }

        var roleRows = await (
                from accountRole in readDbContext.AccountRolesRead
                join role in readDbContext.RolesRead on accountRole.RoleId equals role.Id
                where userIds.Contains(accountRole.UserId)
                select new { accountRole.UserId, RoleName = role.Name! })
            .ToListAsync(cancellationToken);

        return roleRows
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(row => row.RoleName).ToList());
    }
}