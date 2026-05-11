using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UploadAttachment;

public sealed record UploadAttachmentCommand(
    AttachmentOwnerType OwnerType,
    Guid OwnerEntityId,
    string FileName,
    string MimeType,
    Stream Content,
    long SizeBytes)
    : ICommand<AttachmentId>;

public sealed class UploadAttachmentCommandValidator : AbstractValidator<UploadAttachmentCommand>
{
    public UploadAttachmentCommandValidator()
    {
        RuleFor(x => x.OwnerType).IsInEnum();
        RuleFor(x => x.OwnerEntityId).NotEqual(Guid.Empty);
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
        RuleFor(x => x.MimeType).NotEmpty().MaximumLength(127);
        RuleFor(x => x.Content).NotNull();
        RuleFor(x => x.SizeBytes).GreaterThan(0).LessThanOrEqualTo(50 * 1024 * 1024)
            .WithMessage("File size must be between 1 byte and 50 MB.");
    }
}

public sealed class UploadAttachmentCommandHandler(
    MedicalAccessGuard accessGuard,
    IAttachmentRepository attachmentRepository,
    IAppointmentRepository appointmentRepository,
    IExamRepository examRepository,
    IVaccineRepository vaccineRepository,
    IFileStorageService fileStorage)
    : IRequestHandler<UploadAttachmentCommand, AttachmentId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAttachmentRepository _attachmentRepository = attachmentRepository;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;
    private readonly IExamRepository _examRepository = examRepository;
    private readonly IVaccineRepository _vaccineRepository = vaccineRepository;
    private readonly IFileStorageService _fileStorage = fileStorage;

    public async Task<AttachmentId> Handle(
        UploadAttachmentCommand request,
        CancellationToken cancellationToken)
    {
        // Resolve which FamilyMember owns the parent entity (for authorization).
        var ownerMemberId = await ResolveOwnerMemberIdAsync(
            request.OwnerType, request.OwnerEntityId, cancellationToken);

        var (_, requesterMemberId) = await _accessGuard.EnsureCanWriteAsync(
            ownerMemberId, DataCategory.MedicalHistory, cancellationToken);

        // Persist the file first; if domain creation fails we'll need to clean it up.
        var storagePath = await _fileStorage.SaveAsync(
            request.Content, request.FileName, request.MimeType, cancellationToken);

        try
        {
            var attachment = Attachment.Upload(
                request.OwnerEntityId,
                request.OwnerType,
                request.FileName,
                request.MimeType,
                storagePath,
                request.SizeBytes,
                requesterMemberId);

            await _attachmentRepository.AddAsync(attachment, cancellationToken);
            return attachment.Id;
        }
        catch
        {
            // Best-effort cleanup if persisting the entity blows up.
            await _fileStorage.DeleteAsync(storagePath, CancellationToken.None);
            throw;
        }
    }

    private async Task<FamilyMemberId> ResolveOwnerMemberIdAsync(
        AttachmentOwnerType ownerType,
        Guid ownerEntityId,
        CancellationToken cancellationToken)
    {
        return ownerType switch
        {
            AttachmentOwnerType.Appointment => (await _appointmentRepository
                .GetByIdAsync(AppointmentId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Appointment), ownerEntityId)).FamilyMemberId,

            AttachmentOwnerType.Exam => (await _examRepository
                .GetByIdAsync(ExamId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Exam), ownerEntityId)).FamilyMemberId,

            AttachmentOwnerType.Vaccine => (await _vaccineRepository
                .GetByIdAsync(VaccineId.From(ownerEntityId), cancellationToken)
                ?? throw new NotFoundException(nameof(Vaccine), ownerEntityId)).FamilyMemberId,

            _ => throw new ConflictException(
                "attachment.unsupported_owner",
                $"Unsupported attachment owner type: {ownerType}.")
        };
    }
}
