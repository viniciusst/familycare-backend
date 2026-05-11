using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class AppointmentRepository(FamilyCareDbContext dbContext) : IAppointmentRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Appointment?> GetByIdAsync(AppointmentId id, CancellationToken cancellationToken = default)
        => _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<PagedResult<Appointment>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Appointments
            .Where(a => a.FamilyMemberId == memberId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.ScheduledAt)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        return new PagedResult<Appointment>(items, pagination.NormalizedPage, pagination.NormalizedPageSize, totalCount);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
        => await _dbContext.Appointments.AddAsync(appointment, cancellationToken);

    public void Update(Appointment appointment) => _dbContext.Appointments.Update(appointment);

    public void Remove(Appointment appointment) => _dbContext.Appointments.Remove(appointment);
}
