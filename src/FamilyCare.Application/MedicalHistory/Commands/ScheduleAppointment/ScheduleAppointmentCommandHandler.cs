using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<ScheduleAppointmentCommand, AppointmentId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task<AppointmentId> Handle(
        ScheduleAppointmentCommand request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanWriteAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var appointment = Appointment.Schedule(
            request.FamilyMemberId,
            request.ScheduledAt,
            request.Specialty,
            request.DoctorName,
            request.Location,
            request.Notes);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return appointment.Id;
    }
}
