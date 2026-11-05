namespace AuthService.Contracts.Admin;

public sealed record AdminStatsDto(
    int TotalUsers,
    int AdminCount,
    int ModeratorCount,
    int LockedOutCount);
