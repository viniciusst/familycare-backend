using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class ExamRepository(FamilyCareDbContext dbContext) : IExamRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Exam?> GetByIdAsync(ExamId id, CancellationToken cancellationToken = default)
        => _dbContext.Exams.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<PagedResult<Exam>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Exams.Where(e => e.FamilyMemberId == memberId);

        if (fromDate.HasValue)
        {
            query = query.Where(e => e.ExamDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(e => e.ExamDate <= toDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.ExamDate)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Exam>(items, pagination.NormalizedPage, pagination.NormalizedPageSize, totalCount);
    }

    public async Task AddAsync(Exam exam, CancellationToken cancellationToken = default)
        => await _dbContext.Exams.AddAsync(exam, cancellationToken);

    public void Update(Exam exam) => _dbContext.Exams.Update(exam);

    public void Remove(Exam exam) => _dbContext.Exams.Remove(exam);
}
