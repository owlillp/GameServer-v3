using Wolverine;

namespace ProfileService.Infrastructure.Postgres;

public sealed class ProfileWolverineExtension : IWolverineExtension
{
    public void Configure(WolverineOptions options) =>
        options.CodeGeneration.AlwaysUseServiceLocationFor<ProfileServiceDbContext>();
}
