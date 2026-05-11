using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IAllergyRepository
{
    Task<Allergy?> GetByIdAsync(AllergyId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Allergy>> GetByMemberAsync(
        FamilyMemberId memberId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Allergy allergy, CancellationToken cancellationToken = default);

    void Update(Allergy allergy);

    void Remove(Allergy allergy);
}
