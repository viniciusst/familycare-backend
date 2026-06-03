using FamilyCare.Api.IntegrationTests.Infrastructure;

namespace FamilyCare.Api.IntegrationTests.Auth;

[Collection("Api")]
public class LoginEndpointTests(FamilyCareApiFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_WithValidCredentials_Should_ReturnTokens()
    {
        await Client.RegisterAsync("login-ok@familycare.test", "StrongPass1!");

        // Helper unwraps the nested UserId object.
        var tokens = await Client.LoginAsync("login-ok@familycare.test", "StrongPass1!");

        Assert.False(string.IsNullOrEmpty(tokens.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokens.RefreshToken));
        Assert.True(tokens.AccessTokenExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithWrongPassword_Should_Return403()
    {
        await Client.RegisterAsync("login-wrong@familycare.test", "StrongPass1!");

        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("login-wrong@familycare.test", "WrongPass1!"));

        // Same response as "user does not exist" to avoid enumeration.
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_Should_Return403()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("ghost@familycare.test", "StrongPass1!"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
