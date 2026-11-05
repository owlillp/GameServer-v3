using Shared.Core.Abstractions;

namespace AuthService.Core.Features.Admin.Queries.ListUsers;

public sealed record ListUsersQuery(int Page, int PageSize, string? Search) : IQuery;