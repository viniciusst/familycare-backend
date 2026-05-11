using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Application.Identity.Abstractions;

/// <summary>
/// Issues access tokens (JWT) and refresh tokens for authenticated users.
/// Infrastructure provides the implementation.
/// </summary>
public interface IAuthTokenService
{
    /// <summary>
    /// Issues a fresh pair (access + refresh) for the given user.
    /// </summary>
    Task<AuthTokens> IssueTokensAsync(User user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a refresh token: validates and revokes the old, issues a new pair.
    /// </summary>
    Task<AuthTokens> RotateAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a specific refresh token (used by logout).
    /// </summary>
    Task RevokeAsync(string refreshToken, string reason, CancellationToken cancellationToken = default);
}

public sealed record AuthTokens(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    UserId UserId);
