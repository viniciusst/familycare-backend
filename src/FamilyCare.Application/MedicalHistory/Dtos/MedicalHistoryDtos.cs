using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Dtos;

public sealed record AppointmentDto(
    AppointmentId Id,
    FamilyMemberId FamilyMemberId,
    DateTime ScheduledAt,
    string Specialty,
    string DoctorName,
    string? Location,
    string? Notes,
    AppointmentStatus Status);

public sealed record ExamDto(
    ExamId Id,
    FamilyMemberId FamilyMemberId,
    DateOnly ExamDate,
    string ExamType,
    string? Laboratory,
    string? Results,
    string? RequestedBy);

public sealed record VaccineDto(
    VaccineId Id,
    FamilyMemberId FamilyMemberId,
    string Name,
    DateOnly AppliedAt,
    string? Manufacturer,
    string? BatchNumber,
    int? DoseNumber,
    DateOnly? NextDoseDue,
    string? Notes);

public sealed record AllergyDto(
    AllergyId Id,
    FamilyMemberId FamilyMemberId,
    string Substance,
    AllergySeverity Severity,
    string? Reaction,
    DateOnly? FirstObservedAt);

public sealed record ChronicConditionDto(
    ChronicConditionId Id,
    FamilyMemberId FamilyMemberId,
    string Name,
    DateOnly DiagnosedAt,
    string? Notes,
    bool IsActive);

public sealed record AttachmentDto(
    AttachmentId Id,
    Guid OwnerEntityId,
    AttachmentOwnerType OwnerType,
    string FileName,
    string MimeType,
    long SizeBytes,
    FamilyMemberId UploadedByMemberId,
    DateTime UploadedAt);
