using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.CompleteAppointment;

public sealed class CompleteAppointmentCommandHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<CompleteAppointmentCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        await _accessGuard.EnsureCanWriteAsync(
            appointment.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        appointment.MarkCompleted(request.ClosingNotes);
        _appointmentRepository.Update(appointment);
    }
}
