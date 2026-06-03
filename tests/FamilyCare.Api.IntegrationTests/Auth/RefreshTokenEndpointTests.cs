using System.Text.Json;
using FamilyCare.Api.IntegrationTests.Infrastructure;

namespace FamilyCare.Api.IntegrationTests.Auth;

[Collection("Api")]
public class RefreshTokenEndpointTests(FamilyCareApiFactory factory)
    : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Refresh_WithValidToken_Should_ReturnNewPair()
    {
        var (_, tokens) = await Client.RegisterAndLoginAsync("rt-ok@familycare.test");
        Client.ClearAuth(); // refresh endpoint is anonymous

        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshRequest(tokens.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Parse manually because the body contains a strongly-typed UserId.
        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        var newAccessToken = root.GetProperty("accessToken").GetString();
        var newRefreshToken = root.GetProperty("refreshToken").GetString();

        Assert.False(string.IsNullOrEmpty(newAccessToken));
        Assert.False(string.IsNullOrEmpty(newRefreshToken));
        Assert.NotEqual(tokens.RefreshToken, newRefreshToken);
        Assert.NotEqual(tokens.AccessToken, newAccessToken);
    }

    [Fact]
    public async Task Refresh_WithRotatedToken_Should_Return403()
    {
        var (_, tokens) = await Client.RegisterAndLoginAsync("rt-reuse@familycare.test");
        Client.ClearAuth();

        // First refresh succeeds and rotates the token.
        var firstRefresh = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshRequest(tokens.RefreshToken));
        firstRefresh.EnsureSuccessStatusCode();

        // Reusing the OLD token must fail.
        var reused = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshRequest(tokens.RefreshToken));

        Assert.Equal(HttpStatusCode.Forbidden, reused.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithUnknownToken_Should_Return403()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshRequest("this-token-was-never-issued"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
