namespace FamilyCare.Domain.Tests.Identity;

public class RefreshTokenTests
{
    private const string AnyHash = "abc123hash";

    [Fact]
    public void Issue_ShouldCreateActiveToken()
    {
        var userId = UserId.New();
        var lifetime = TimeSpan.FromDays(30);

        var token = RefreshToken.Issue(userId, AnyHash, lifetime);

        Assert.Equal(userId, token.UserId);
        Assert.Equal(AnyHash, token.TokenHash);
        Assert.Null(token.RevokedAt);
        Assert.Null(token.ReplacedByTokenId);
        Assert.InRange(
            token.ExpiresAt,
            DateTime.UtcNow.Add(lifetime).AddSeconds(-5),
            DateTime.UtcNow.Add(lifetime).AddSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Issue_WithEmptyHash_ShouldThrow(string? hash)
    {
        Assert.Throws<InvalidEntityStateException>(
            () => RefreshToken.Issue(UserId.New(), hash!, TimeSpan.FromDays(1)));
    }

    [Fact]
    public void Issue_WithNonPositiveLifetime_ShouldThrow()
    {
        Assert.Throws<InvalidEntityStateException>(
            () => RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.Zero));
    }

    [Fact]
    public void IsActive_WhenNotExpiredAndNotRevoked_ShouldReturnTrue()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromMinutes(10));

        Assert.True(token.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void IsActive_WhenExpired_ShouldReturnFalse()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromMinutes(10));

        Assert.False(token.IsActive(DateTime.UtcNow.AddDays(1)));
    }

    [Fact]
    public void IsActive_WhenRevoked_ShouldReturnFalse()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromDays(30));
        token.Revoke("test");

        Assert.False(token.IsActive(DateTime.UtcNow));
    }

    [Fact]
    public void RevokeAndReplace_ShouldMarkRevokedWithReplacement()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromDays(30));
        var replacementId = RefreshTokenId.New();

        token.RevokeAndReplace(replacementId, "rotated");

        Assert.NotNull(token.RevokedAt);
        Assert.Equal(replacementId, token.ReplacedByTokenId);
        Assert.Equal("rotated", token.RevokedReason);
    }

    [Fact]
    public void RevokeAndReplace_OnAlreadyRevoked_ShouldThrow()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromDays(30));
        token.Revoke("first");

        Assert.Throws<BusinessRuleViolationException>(
            () => token.RevokeAndReplace(RefreshTokenId.New(), "second"));
    }

    [Fact]
    public void Revoke_IsIdempotent()
    {
        var token = RefreshToken.Issue(UserId.New(), AnyHash, TimeSpan.FromDays(30));

        token.Revoke("first");
        var firstRevokedAt = token.RevokedAt;
        token.Revoke("second");

        Assert.Equal(firstRevokedAt, token.RevokedAt);
        Assert.Equal("first", token.RevokedReason);
    }
}
