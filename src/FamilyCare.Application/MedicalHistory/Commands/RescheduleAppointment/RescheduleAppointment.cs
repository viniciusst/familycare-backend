using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.RescheduleAppointment;

public sealed record RescheduleAppointmentCommand(
    AppointmentId AppointmentId,
    DateTime NewScheduledAt)
    : ICommand;

public sealed class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.NewScheduledAt).NotEmpty();
    }
}

public sealed class RescheduleAppointmentCommandHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<RescheduleAppointmentCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        await _accessGuard.EnsureCanWriteAsync(
            appointment.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        appointment.Reschedule(request.NewScheduledAt);
        _appointmentRepository.Update(appointment);
    }
}
