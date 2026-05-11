using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class ChronicConditionRepository(FamilyCareDbContext dbContext) : IChronicConditionRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<ChronicCondition?> GetByIdAsync(ChronicConditionId id, CancellationToken cancellationToken = default)
        => _dbContext.ChronicConditions.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ChronicCondition>> GetByMemberAsync(
        FamilyMemberId memberId,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ChronicConditions.Where(c => c.FamilyMemberId == memberId);

        if (activeOnly.HasValue)
        {
            query = query.Where(c => c.IsActive == activeOnly.Value);
        }

        return await query
            .OrderByDescending(c => c.IsActive)
            .ThenByDescending(c => c.DiagnosedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ChronicCondition condition, CancellationToken cancellationToken = default)
        => await _dbContext.ChronicConditions.AddAsync(condition, cancellationToken);

    public void Update(ChronicCondition condition) => _dbContext.ChronicConditions.Update(condition);

    public void Remove(ChronicCondition condition) => _dbContext.ChronicConditions.Remove(condition);
}
