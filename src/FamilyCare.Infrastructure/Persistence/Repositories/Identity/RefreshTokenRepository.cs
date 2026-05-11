using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.Identity;

public sealed class RefreshTokenRepository(FamilyCareDbContext dbContext) : IRefreshTokenRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
        => _dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbContext.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > now)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        => await _dbContext.RefreshTokens.AddAsync(token, cancellationToken);

    public void Update(RefreshToken token) => _dbContext.RefreshTokens.Update(token);

    public async Task RevokeAllByUserAsync(
        UserId userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var activeTokens = await GetActiveByUserAsync(userId, cancellationToken);
        foreach (var token in activeTokens)
        {
            token.Revoke(reason);
        }
    }
}
