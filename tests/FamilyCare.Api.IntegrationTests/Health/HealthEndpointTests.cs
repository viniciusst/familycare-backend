using FamilyCare.Api.IntegrationTests.Infrastructure;

namespace FamilyCare.Api.IntegrationTests.Health;

[Collection("Api")]
public class HealthEndpointTests(FamilyCareApiFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Health_Should_Return200()
    {
        var response = await Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task HealthReady_Should_Return200_WhenDatabaseReachable()
    {
        var response = await Client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
