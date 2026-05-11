using FamilyCare.Application.Common.Pagination;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IVaccineRepository
{
    Task<Vaccine?> GetByIdAsync(VaccineId id, CancellationToken cancellationToken = default);

    Task<PagedResult<Vaccine>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        CancellationToken cancellationToken = default);

    /// <summary>Returns vaccines whose NextDoseDue is approaching within the given window.</summary>
    Task<IReadOnlyList<Vaccine>> GetUpcomingDosesAsync(
        FamilyMemberId memberId,
        DateOnly until,
        CancellationToken cancellationToken = default);

    Task AddAsync(Vaccine vaccine, CancellationToken cancellationToken = default);

    void Update(Vaccine vaccine);

    void Remove(Vaccine vaccine);
}
