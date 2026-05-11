using System.Security.Cryptography;
using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Identity;
using FamilyCare.Infrastructure.Identity.Authentication;
using Microsoft.Extensions.Options;

namespace FamilyCare.Infrastructure.Identity.Services;

/// <summary>
/// Full implementation of IAuthTokenService:
/// - Issues JWT access tokens (HMAC-SHA256) and opaque refresh tokens (256-bit random)
/// - Stores only SHA-256 hashes of refresh tokens in DB
/// - Rotates refresh tokens on every use (anti-replay)
/// - Detects reuse of already-rotated tokens and revokes the whole chain
/// </summary>
public sealed class AuthTokenService(
    IJwtTokenService jwtTokenService,
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IOptions<JwtOptions> jwtOptions)
    : IAuthTokenService
{
    private readonly IJwtTokenService _jwtTokenService = jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository = refreshTokenRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthTokens> IssueTokensAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var (rawRefreshToken, hash) = GenerateRefreshToken();
        var lifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);

        var refreshTokenEntity = RefreshToken.Issue(user.Id, hash, lifetime);
        await _refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var access = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.PreferredLanguage.ToString());

        return new AuthTokens(
            access.Token,
            access.ExpiresAt,
            rawRefreshToken,
            refreshTokenEntity.ExpiresAt,
            user.Id);
    }

    public async Task<AuthTokens> RotateAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ForbiddenException("Invalid refresh token.");
        }

        var hash = HashToken(refreshToken);

        var existing = await _refreshTokenRepository.GetByHashAsync(hash, cancellationToken)
            ?? throw new ForbiddenException("Invalid refresh token.");

        var now = DateTime.UtcNow;

        // Reuse of an already-rotated token: someone got hold of a leaked token.
        // Revoke ALL the user's tokens as a precaution.
        if (existing.RevokedAt is not null)
        {
            await _refreshTokenRepository.RevokeAllByUserAsync(
                existing.UserId, "reuse_detected", cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw new ForbiddenException("Refresh token was already used. All sessions revoked.");
        }

        if (!existing.IsActive(now))
        {
            throw new ForbiddenException("Refresh token expired.");
        }

        var user = await _userRepository.GetByIdAsync(existing.UserId, cancellationToken)
            ?? throw new ForbiddenException("Invalid refresh token.");

        // Issue new pair
        var (newRawToken, newHash) = GenerateRefreshToken();
        var lifetime = TimeSpan.FromDays(_jwtOptions.RefreshTokenDays);
        var newRefreshToken = RefreshToken.Issue(user.Id, newHash, lifetime);

        existing.RevokeAndReplace(newRefreshToken.Id, "rotated");

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        _refreshTokenRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var access = _jwtTokenService.IssueAccessToken(
            user.Id, user.Email.Value, user.PreferredLanguage.ToString());

        return new AuthTokens(
            access.Token,
            access.ExpiresAt,
            newRawToken,
            newRefreshToken.ExpiresAt,
            user.Id);
    }

    public async Task RevokeAsync(string refreshToken, string reason, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var hash = HashToken(refreshToken);
        var existing = await _refreshTokenRepository.GetByHashAsync(hash, cancellationToken);

        if (existing is null || existing.RevokedAt is not null)
        {
            return;
        }

        existing.Revoke(reason);
        _refreshTokenRepository.Update(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Generates a 256-bit cryptographically random token and its SHA-256 hash.
    /// The raw token is returned to the client; only the hash is stored in DB.
    /// </summary>
    private static (string Raw, string Hash) GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(randomBytes);
        var hash = HashToken(raw);
        return (raw, hash);
    }

    private static string HashToken(string raw)
    {
        var hashBytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hashBytes);
    }
}
