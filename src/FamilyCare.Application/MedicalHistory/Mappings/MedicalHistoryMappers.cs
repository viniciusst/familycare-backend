using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Mappings;

internal static class MedicalHistoryMappers
{
    public static AppointmentDto ToDto(this Appointment a)
        => new(a.Id, a.FamilyMemberId, a.ScheduledAt, a.Specialty, a.DoctorName,
               a.Location, a.Notes, a.Status);

    public static ExamDto ToDto(this Exam e)
        => new(e.Id, e.FamilyMemberId, e.ExamDate, e.ExamType,
               e.Laboratory, e.Results, e.RequestedBy);

    public static VaccineDto ToDto(this Vaccine v)
        => new(v.Id, v.FamilyMemberId, v.Name, v.AppliedAt,
               v.Manufacturer, v.BatchNumber, v.DoseNumber, v.NextDoseDue, v.Notes);

    public static AllergyDto ToDto(this Allergy a)
        => new(a.Id, a.FamilyMemberId, a.Substance, a.Severity, a.Reaction, a.FirstObservedAt);

    public static ChronicConditionDto ToDto(this ChronicCondition c)
        => new(c.Id, c.FamilyMemberId, c.Name, c.DiagnosedAt, c.Notes, c.IsActive);

    public static AttachmentDto ToDto(this Attachment att)
        => new(att.Id, att.OwnerEntityId, att.OwnerType, att.FileName, att.MimeType,
               att.SizeBytes, att.UploadedByMemberId, att.UploadedAt);
}
