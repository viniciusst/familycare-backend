using FamilyCare.Application.Common.Pagination;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(AppointmentId id, CancellationToken cancellationToken = default);

    Task<PagedResult<Appointment>> GetByMemberAsync(
        FamilyMemberId memberId,
        PagedRequest pagination,
        AppointmentStatus? status = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    void Update(Appointment appointment);

    void Remove(Appointment appointment);
}
