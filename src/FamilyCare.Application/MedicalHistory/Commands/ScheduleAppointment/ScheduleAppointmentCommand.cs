using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.MedicalHistory.Commands.ScheduleAppointment;

public sealed record ScheduleAppointmentCommand(
    FamilyMemberId FamilyMemberId,
    DateTime ScheduledAt,
    string Specialty,
    string DoctorName,
    string? Location = null,
    string? Notes = null)
    : ICommand<AppointmentId>;
