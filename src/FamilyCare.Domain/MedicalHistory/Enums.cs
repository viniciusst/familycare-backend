namespace FamilyCare.Domain.MedicalHistory;

public enum AppointmentStatus
{
    Scheduled = 1,
    Completed = 2,
    Cancelled = 3,
    NoShow = 4
}

public enum AllergySeverity
{
    Mild = 1,
    Moderate = 2,
    Severe = 3,
    LifeThreatening = 4
}

/// <summary>Discriminator for the polymorphic Attachment entity.</summary>
public enum AttachmentOwnerType
{
    Appointment = 1,
    Exam = 2,
    Vaccine = 3
    // Reserved for future contexts: Medication = 10, SymptomLog = 20, ...
}
