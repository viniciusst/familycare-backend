using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

public sealed class Appointment : AggregateRoot<AppointmentId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public DateTime ScheduledAt { get; private set; }
    public string Specialty { get; private set; }
    public string DoctorName { get; private set; }
    public string? Location { get; private set; }
    public string? Notes { get; private set; }
    public AppointmentStatus Status { get; private set; }

    private Appointment() : base()
    {
        Specialty = null!;
        DoctorName = null!;
    }

    private Appointment(
        AppointmentId id,
        FamilyMemberId memberId,
        DateTime scheduledAt,
        string specialty,
        string doctorName,
        string? location,
        string? notes) : base(id)
    {
        FamilyMemberId = memberId;
        ScheduledAt = scheduledAt;
        Specialty = ValidateRequired(specialty, 80, "appointment.specialty_required", "appointment.specialty_too_long", "Specialty");
        DoctorName = ValidateRequired(doctorName, 120, "appointment.doctor_required", "appointment.doctor_too_long", "Doctor name");
        Location = TrimOrNull(location);
        Notes = TrimOrNull(notes);
        Status = AppointmentStatus.Scheduled;
    }

    public static Appointment Schedule(
        FamilyMemberId memberId,
        DateTime scheduledAt,
        string specialty,
        string doctorName,
        string? location = null,
        string? notes = null)
    {
        var appt = new Appointment(
            AppointmentId.New(), memberId, scheduledAt, specialty, doctorName, location, notes);

        appt.RaiseDomainEvent(new AppointmentScheduledEvent(
            appt.Id, memberId, scheduledAt, appt.Specialty, DateTime.UtcNow));

        return appt;
    }

    public void MarkCompleted(string? closingNotes = null)
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.Completed;
        if (!string.IsNullOrWhiteSpace(closingNotes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes)
                ? closingNotes.Trim()
                : $"{Notes}\n---\n{closingNotes.Trim()}";
        }
        RaiseDomainEvent(new AppointmentCompletedEvent(Id, FamilyMemberId, DateTime.UtcNow));
    }

    public void Cancel()
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.Cancelled;
        RaiseDomainEvent(new AppointmentCancelledEvent(Id, FamilyMemberId, DateTime.UtcNow));
    }

    public void MarkNoShow()
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        Status = AppointmentStatus.NoShow;
    }

    public void Reschedule(DateTime newScheduledAt)
    {
        EnsureStatus(AppointmentStatus.Scheduled);
        ScheduledAt = newScheduledAt;
    }

    public void UpdateDetails(
    string? doctorName,
    string? specialty,
    string? location,
    string? notes)
    {
        EnsureStatus(AppointmentStatus.Scheduled);

        if (doctorName is null && specialty is null && location is null && notes is null)
        {
            throw new BusinessRuleViolationException(
                "appointment.no_fields_to_update",
                "At least one field must be provided to update.");
        }

        if (doctorName is not null)
        {
            DoctorName = ValidateRequired(
                doctorName, 120,
                "appointment.doctor_required",
                "appointment.doctor_too_long",
                "Doctor name");
        }

        if (specialty is not null)
        {
            Specialty = ValidateRequired(
                specialty, 80,
                "appointment.specialty_required",
                "appointment.specialty_too_long",
                "Specialty");
        }

        if (location is not null)
        {
            if (location.Length > 200)
            {
                throw new InvalidEntityStateException(
                    "appointment.location_too_long",
                    "Location exceeds 200 characters.");
            }
            Location = TrimOrNull(location);
        }

        if (notes is not null)
        {
            if (notes.Length > 2000)
            {
                throw new InvalidEntityStateException(
                    "appointment.notes_too_long",
                    "Notes exceed 2000 characters.");
            }
            Notes = TrimOrNull(notes);
        }

        RaiseDomainEvent(new AppointmentDetailsUpdatedEvent(
            Id, FamilyMemberId, DateTime.UtcNow));
    }

    private void EnsureStatus(AppointmentStatus expected)
    {
        if (Status != expected)
        {
            throw new BusinessRuleViolationException(
                "appointment.invalid_status_transition",
                $"Operation requires status '{expected}', but is '{Status}'.");
        }
    }

    private static string ValidateRequired(string value, int maxLength, string requiredCode, string lengthCode, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidEntityStateException(requiredCode, $"{fieldName} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new InvalidEntityStateException(lengthCode, $"{fieldName} exceeds {maxLength} characters.");
        }

        return trimmed;
    }

    private static string? TrimOrNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
