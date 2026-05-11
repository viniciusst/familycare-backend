using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Application.Identity.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);

    void Update(RefreshToken token);

    /// <summary>Revokes all active tokens for a user (e.g. on password change or logout-all).</summary>
    Task RevokeAllByUserAsync(UserId userId, string reason, CancellationToken cancellationToken = default);
}
