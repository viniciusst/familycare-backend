using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.MedicalHistory.Commands.CompleteAppointment;

public sealed record CompleteAppointmentCommand(
    AppointmentId AppointmentId,
    string? ClosingNotes = null)
    : ICommand;
