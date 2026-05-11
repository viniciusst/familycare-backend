using FluentValidation;

namespace FamilyCare.Application.MedicalHistory.Commands.ScheduleAppointment;

public sealed class ScheduleAppointmentCommandValidator : AbstractValidator<ScheduleAppointmentCommand>
{
    public ScheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.Specialty).NotEmpty().MaximumLength(80);
        RuleFor(x => x.DoctorName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ScheduledAt).NotEmpty();
    }
}
