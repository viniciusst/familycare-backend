using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.MedicalHistory.Events;

public sealed record AppointmentScheduledEvent(
    AppointmentId AppointmentId,
    FamilyMemberId FamilyMemberId,
    DateTime ScheduledAt,
    string Specialty,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AppointmentCompletedEvent(
    AppointmentId AppointmentId,
    FamilyMemberId FamilyMemberId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AppointmentCancelledEvent(
    AppointmentId AppointmentId,
    FamilyMemberId FamilyMemberId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record ExamRegisteredEvent(
    ExamId ExamId,
    FamilyMemberId FamilyMemberId,
    DateOnly ExamDate,
    string ExamType,
    DateTime OccurredOn) : IDomainEvent;

public sealed record VaccineRegisteredEvent(
    VaccineId VaccineId,
    FamilyMemberId FamilyMemberId,
    string Name,
    DateOnly AppliedAt,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AllergyRegisteredEvent(
    AllergyId AllergyId,
    FamilyMemberId FamilyMemberId,
    string Substance,
    AllergySeverity Severity,
    DateTime OccurredOn) : IDomainEvent;

public sealed record ChronicConditionRegisteredEvent(
    ChronicConditionId ChronicConditionId,
    FamilyMemberId FamilyMemberId,
    string Name,
    DateTime OccurredOn) : IDomainEvent;

public sealed record AttachmentUploadedEvent(
    AttachmentId AttachmentId,
    AttachmentOwnerType OwnerType,
    Guid OwnerEntityId,
    string FileName,
    DateTime OccurredOn) : IDomainEvent;
