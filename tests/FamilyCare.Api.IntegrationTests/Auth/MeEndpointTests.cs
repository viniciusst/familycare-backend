using FamilyCare.Api.IntegrationTests.Infrastructure;

namespace FamilyCare.Api.IntegrationTests.Auth;

[Collection("Api")]
public class MeEndpointTests(FamilyCareApiFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task GetMe_WithBearer_Should_ReturnProfile()
    {
        var (user, _) = await Client.RegisterAndLoginAsync("me@familycare.test");

        // Use the helper that parses strongly-typed IDs.
        var me = await Client.GetMeAsync();

        Assert.Equal(user.UserId, me.Id);
        Assert.Equal("me@familycare.test", me.Email);
    }

    [Fact]
    public async Task GetMe_WithoutBearer_Should_Return401()
    {
        var response = await Client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_WithInvalidBearer_Should_Return401()
    {
        Client.SetBearer("garbage-not-a-jwt");

        var response = await Client.GetAsync("/api/v1/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
