using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UpdateAppointmentDetails;

/// <summary>
/// Updates editable details of a scheduled appointment (doctor name,
/// specialty, location, notes). To change date/time use Reschedule
/// instead; to change state use Complete/Cancel.
/// All fields are optional but at least one must be provided.
/// </summary>
public sealed record UpdateAppointmentDetailsCommand(
    AppointmentId AppointmentId,
    string? DoctorName,
    string? Specialty,
    string? Location,
    string? Notes)
    : ICommand;

public sealed class UpdateAppointmentDetailsCommandValidator
    : AbstractValidator<UpdateAppointmentDetailsCommand>
{
    public UpdateAppointmentDetailsCommandValidator()
    {
        When(x => x.DoctorName is not null, () =>
        {
            RuleFor(x => x.DoctorName!)
                .NotEmpty()
                .MaximumLength(120);
        });

        When(x => x.Specialty is not null, () =>
        {
            RuleFor(x => x.Specialty!)
                .NotEmpty()
                .MaximumLength(80);
        });

        When(x => x.Location is not null, () =>
        {
            RuleFor(x => x.Location!)
                .MaximumLength(200);
        });

        When(x => x.Notes is not null, () =>
        {
            RuleFor(x => x.Notes!)
                .MaximumLength(2000);
        });

        // At least one field must be provided.
        RuleFor(x => x)
            .Must(x =>
                x.DoctorName is not null ||
                x.Specialty is not null ||
                x.Location is not null ||
                x.Notes is not null)
            .WithMessage("At least one field must be provided to update.");
    }
}

public sealed class UpdateAppointmentDetailsCommandHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<UpdateAppointmentDetailsCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task Handle(
        UpdateAppointmentDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(
            request.AppointmentId, cancellationToken)
            ?? throw new NotFoundException(nameof(Appointment), request.AppointmentId);

        await _accessGuard.EnsureCanWriteAsync(
            appointment.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        appointment.UpdateDetails(
            request.DoctorName,
            request.Specialty,
            request.Location,
            request.Notes);

        _appointmentRepository.Update(appointment);
    }
}