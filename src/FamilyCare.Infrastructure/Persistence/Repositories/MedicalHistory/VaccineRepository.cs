using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class VaccineRepository(FamilyCareDbContext dbContext) : IVaccineRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Vaccine?> GetByIdAsync(VaccineId id, CancellationToken cancellationToken = default)
        => _dbContext.Vaccines.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<PagedResult<Vaccine>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Vaccines.Where(v => v.FamilyMemberId == memberId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(v => v.AppliedAt)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Vaccine>(items, pagination.NormalizedPage, pagination.NormalizedPageSize, totalCount);
    }

    public async Task<IReadOnlyList<Vaccine>> GetUpcomingDosesAsync(
        FamilyMemberId memberId,
        DateOnly until,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Vaccines
            .Where(v => v.FamilyMemberId == memberId
                     && v.NextDoseDue != null
                     && v.NextDoseDue <= until)
            .OrderBy(v => v.NextDoseDue)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Vaccine vaccine, CancellationToken cancellationToken = default)
        => await _dbContext.Vaccines.AddAsync(vaccine, cancellationToken);

    public void Update(Vaccine vaccine) => _dbContext.Vaccines.Update(vaccine);

    public void Remove(Vaccine vaccine) => _dbContext.Vaccines.Remove(vaccine);
}
