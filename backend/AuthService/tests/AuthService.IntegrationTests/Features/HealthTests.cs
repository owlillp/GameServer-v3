using System.Net;
using AuthService.IntegrationTests.Infrastructure;

namespace AuthService.IntegrationTests.Features;

public sealed class HealthTests : IntegrationTestsBase
{
    public HealthTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        HttpResponseMessage response = await HttpClient.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
