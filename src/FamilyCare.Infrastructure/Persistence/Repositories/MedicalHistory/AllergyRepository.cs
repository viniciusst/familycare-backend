using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class AllergyRepository(FamilyCareDbContext dbContext) : IAllergyRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Allergy?> GetByIdAsync(AllergyId id, CancellationToken cancellationToken = default)
        => _dbContext.Allergies.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Allergy>> GetByMemberAsync(
        FamilyMemberId memberId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Allergies
            .Where(a => a.FamilyMemberId == memberId)
            .OrderByDescending(a => a.Severity)
            .ThenBy(a => a.Substance)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Allergy allergy, CancellationToken cancellationToken = default)
        => await _dbContext.Allergies.AddAsync(allergy, cancellationToken);

    public void Update(Allergy allergy) => _dbContext.Allergies.Update(allergy);

    public void Remove(Allergy allergy) => _dbContext.Allergies.Remove(allergy);
}
