using Wolverine;

namespace AuthService.Infrastructure.Postgres;

public sealed class AuthWolverineExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options) =>
        options.CodeGeneration.AlwaysUseServiceLocationFor<AuthServiceDbContext>();
}
