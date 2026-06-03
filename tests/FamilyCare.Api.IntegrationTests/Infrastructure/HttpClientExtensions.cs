using System.Net.Http.Headers;
using System.Text.Json;

namespace FamilyCare.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Common HTTP request payloads, mirroring the API's DTO shape. We don't
/// import the production types directly because we want the tests to fail
/// loudly if the wire contract changes accidentally.
///
/// Note: the API serializes strongly-typed IDs (UserId, FamilyId, etc.) as
/// nested objects with a "value" property. The helpers below unwrap that
/// shape for us so test bodies can work with plain <see cref="Guid"/> values.
/// </summary>
internal sealed record RegisterRequest(string Email, string Password, int PreferredLanguage);

internal sealed record RegisterResponse(Guid UserId, string Email);

internal sealed record LoginRequest(string Email, string Password);

internal sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid UserId);

internal sealed record RefreshRequest(string RefreshToken);

internal sealed record LogoutRequest(string RefreshToken);

internal sealed record MeResponse(
    Guid Id,
    string Email,
    int PreferredLanguage,
    DateTime CreatedAt);

internal sealed record ProblemDetailsResponse(
    string Type,
    string Title,
    int Status,
    string? Code,
    string? Detail);

/// <summary>
/// Fluent helpers for common auth flows. Keeps test bodies focused on the
/// behavior being asserted instead of repeating six lines of register+login.
/// </summary>
internal static class HttpClientExtensions
{
    private const int EnglishCanada = 2; // SupportedLanguage enum value

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<RegisterResponse> RegisterAsync(
        this HttpClient client,
        string email,
        string password = "StrongPass1!",
        int preferredLanguage = EnglishCanada)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest(email, password, preferredLanguage));

        response.EnsureSuccessStatusCode();

        // Parse the response manually because strongly-typed IDs serialize
        // as nested objects with a "value" property.
        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);

        var userId = ExtractStronglyTypedId(doc.RootElement, "userId");
        var emailValue = doc.RootElement.GetProperty("email").GetString()!;

        return new RegisterResponse(userId, emailValue);
    }

    public static async Task<AuthTokens> LoginAsync(
        this HttpClient client,
        string email,
        string password = "StrongPass1!")
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(email, password));

        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        return new AuthTokens(
            AccessToken: root.GetProperty("accessToken").GetString()!,
            AccessTokenExpiresAt: root.GetProperty("accessTokenExpiresAt").GetDateTime(),
            RefreshToken: root.GetProperty("refreshToken").GetString()!,
            RefreshTokenExpiresAt: root.GetProperty("refreshTokenExpiresAt").GetDateTime(),
            UserId: ExtractStronglyTypedId(root, "userId"));
    }

    public static async Task<MeResponse> GetMeAsync(this HttpClient client)
    {
        var response = await client.GetAsync("/api/v1/auth/me");
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        return new MeResponse(
            Id: ExtractStronglyTypedId(root, "id"),
            Email: root.GetProperty("email").GetString()!,
            PreferredLanguage: root.GetProperty("preferredLanguage").GetInt32(),
            CreatedAt: root.GetProperty("createdAt").GetDateTime());
    }

    /// <summary>
    /// Registers + logs in + sets Bearer auth header on the client. Returns
    /// both the registration response and the tokens for any further inspection.
    /// </summary>
    public static async Task<(RegisterResponse User, AuthTokens Tokens)>
        RegisterAndLoginAsync(
            this HttpClient client,
            string email,
            string password = "StrongPass1!")
    {
        var user = await client.RegisterAsync(email, password);
        var tokens = await client.LoginAsync(email, password);
        client.SetBearer(tokens.AccessToken);
        return (user, tokens);
    }

    public static void SetBearer(this HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public static void ClearAuth(this HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization = null;
    }

    // Unwraps a strongly-typed ID. The API serializes UserId / FamilyId / etc.
    // as either a nested object { "userId": { "value": "..." } } OR a flat
    // string { "userId": "..." }. We support both because the wire format may
    // evolve (e.g. once a JsonConverter is added on the production side).
    private static Guid ExtractStronglyTypedId(JsonElement parent, string propertyName)
    {
        var prop = parent.GetProperty(propertyName);

        return prop.ValueKind switch
        {
            JsonValueKind.String => prop.GetGuid(),
            JsonValueKind.Object => prop.GetProperty("value").GetGuid(),
            _ => throw new JsonException(
                $"Unexpected JSON shape for strongly-typed id '{propertyName}': {prop.ValueKind}"),
        };
    }
}
