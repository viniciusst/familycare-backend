using FamilyCare.Domain.Common;

namespace FamilyCare.Infrastructure.Identity.Authentication;

/// <summary>
/// Generates JWT access tokens for authenticated users.
/// (Refresh-token storage will be added when needed.)
/// </summary>
public interface IJwtTokenService
{
    JwtAccessToken IssueAccessToken(UserId userId, string email, string preferredLanguage);
}

public sealed record JwtAccessToken(string Token, DateTime ExpiresAt);
