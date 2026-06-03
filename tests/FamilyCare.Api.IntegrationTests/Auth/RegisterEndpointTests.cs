using FamilyCare.Api.IntegrationTests.Infrastructure;

namespace FamilyCare.Api.IntegrationTests.Auth;

[Collection("Api")]
public class RegisterEndpointTests(FamilyCareApiFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_WithValidPayload_Should_Return201AndUserId()
    {
        // We use the helper because the server serializes strongly-typed IDs
        // as nested { "value": "<guid>" } objects, which plain
        // ReadFromJsonAsync<RegisterResponse> cannot parse.
        var body = await Client.RegisterAsync(
            "vinicius@familycare.test", "StrongPass1!", preferredLanguage: 1);

        Assert.NotEqual(Guid.Empty, body.UserId);
        Assert.Equal("vinicius@familycare.test", body.Email);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Should_Return409Conflict()
    {
        await Client.RegisterAsync("dupe@familycare.test");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest("dupe@familycare.test", "StrongPass1!", 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Should_Return400WithValidationDetails()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest("weakpw@familycare.test", "short", 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem!.Status);
    }
}
