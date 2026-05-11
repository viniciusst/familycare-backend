using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IChronicConditionRepository
{
    Task<ChronicCondition?> GetByIdAsync(ChronicConditionId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChronicCondition>> GetByMemberAsync(
        FamilyMemberId memberId,
        bool? activeOnly = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(ChronicCondition condition, CancellationToken cancellationToken = default);

    void Update(ChronicCondition condition);

    void Remove(ChronicCondition condition);
}
