using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

/// <summary>
/// Polymorphic attachment. <see cref="OwnerEntityId"/> + <see cref="OwnerType"/>
/// identify the entity it belongs to. Cascade cleanup is handled via
/// domain events when the owner is deleted (no FK at the database level).
/// </summary>
public sealed class Attachment : AggregateRoot<AttachmentId>
{
    public Guid OwnerEntityId { get; private set; }
    public AttachmentOwnerType OwnerType { get; private set; }
    public string FileName { get; private set; }
    public string MimeType { get; private set; }
    public string StoragePath { get; private set; }
    public long SizeBytes { get; private set; }
    public FamilyMemberId UploadedByMemberId { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private Attachment() : base()
    {
        FileName = null!;
        MimeType = null!;
        StoragePath = null!;
    }

    private Attachment(
        AttachmentId id,
        Guid ownerEntityId,
        AttachmentOwnerType ownerType,
        string fileName,
        string mimeType,
        string storagePath,
        long sizeBytes,
        FamilyMemberId uploadedByMemberId,
        DateTime uploadedAt) : base(id)
    {
        if (ownerEntityId == Guid.Empty)
        {
            throw new InvalidEntityStateException(
                "attachment.owner_required",
                "Owner entity id is required.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new InvalidEntityStateException(
                "attachment.filename_required",
                "File name is required.");
        }

        if (fileName.Length > 255)
        {
            throw new InvalidEntityStateException(
                "attachment.filename_too_long",
                "File name exceeds 255 characters.");
        }

        if (string.IsNullOrWhiteSpace(mimeType))
        {
            throw new InvalidEntityStateException(
                "attachment.mimetype_required",
                "MIME type is required.");
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new InvalidEntityStateException(
                "attachment.storage_path_required",
                "Storage path is required.");
        }

        if (sizeBytes <= 0)
        {
            throw new InvalidEntityStateException(
                "attachment.invalid_size",
                "Size must be greater than zero.");
        }

        OwnerEntityId = ownerEntityId;
        OwnerType = ownerType;
        FileName = fileName.Trim();
        MimeType = mimeType.Trim();
        StoragePath = storagePath.Trim();
        SizeBytes = sizeBytes;
        UploadedByMemberId = uploadedByMemberId;
        UploadedAt = uploadedAt;
    }

    public static Attachment Upload(
        Guid ownerEntityId,
        AttachmentOwnerType ownerType,
        string fileName,
        string mimeType,
        string storagePath,
        long sizeBytes,
        FamilyMemberId uploadedByMemberId)
    {
        var now = DateTime.UtcNow;
        var attachment = new Attachment(
            AttachmentId.New(),
            ownerEntityId,
            ownerType,
            fileName,
            mimeType,
            storagePath,
            sizeBytes,
            uploadedByMemberId,
            now);

        attachment.RaiseDomainEvent(new AttachmentUploadedEvent(
            attachment.Id, ownerType, ownerEntityId, attachment.FileName, now));

        return attachment;
    }
}
