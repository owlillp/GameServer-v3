namespace AuthService.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class IntegrationTestFixture : ICollectionFixture<IntegrationTestsWebFactory>
{
    public const string Name = "integration";
}
