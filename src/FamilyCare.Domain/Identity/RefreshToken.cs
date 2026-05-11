using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.Identity;

/// <summary>
/// Refresh token issued to a user. Stores the SHA-256 hash of the token
/// (never the plaintext) so a leaked database doesn't grant authentication.
/// Supports rotation (each use creates a new token and revokes the previous)
/// and explicit revocation (logout, password change).
/// </summary>
public sealed class RefreshToken : AggregateRoot<RefreshTokenId>
{
    public UserId UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public RefreshTokenId? ReplacedByTokenId { get; private set; }
    public string? RevokedReason { get; private set; }

    private RefreshToken() : base()
    {
        TokenHash = null!;
    }

    private RefreshToken(
        RefreshTokenId id,
        UserId userId,
        string tokenHash,
        DateTime createdAt,
        DateTime expiresAt) : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Issue(UserId userId, string tokenHash, TimeSpan lifetime)
    {
        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new InvalidEntityStateException(
                "refresh_token.hash_required",
                "Token hash is required.");
        }

        if (lifetime <= TimeSpan.Zero)
        {
            throw new InvalidEntityStateException(
                "refresh_token.invalid_lifetime",
                "Token lifetime must be positive.");
        }

        var now = DateTime.UtcNow;
        return new RefreshToken(RefreshTokenId.New(), userId, tokenHash, now, now.Add(lifetime));
    }

    public bool IsActive(DateTime nowUtc)
        => RevokedAt is null && nowUtc < ExpiresAt;

    public void RevokeAndReplace(RefreshTokenId replacement, string reason)
    {
        if (RevokedAt is not null)
        {
            throw new BusinessRuleViolationException(
                "refresh_token.already_revoked",
                "Refresh token has already been revoked.");
        }

        RevokedAt = DateTime.UtcNow;
        ReplacedByTokenId = replacement;
        RevokedReason = reason;
    }

    public void Revoke(string reason)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }
}
