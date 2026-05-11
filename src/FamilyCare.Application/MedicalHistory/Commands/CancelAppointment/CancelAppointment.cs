using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(AppointmentId AppointmentId) : ICommand;

public sealed class CancelAppointmentCommandHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<CancelAppointmentCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        await _accessGuard.EnsureCanWriteAsync(
            appointment.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        appointment.Cancel();
        _appointmentRepository.Update(appointment);
    }
}
