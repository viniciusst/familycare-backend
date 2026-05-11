using FamilyCare.Application.Common.Pagination;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(ExamId id, CancellationToken cancellationToken = default);

    Task<PagedResult<Exam>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Exam exam, CancellationToken cancellationToken = default);

    void Update(Exam exam);

    void Remove(Exam exam);
}
