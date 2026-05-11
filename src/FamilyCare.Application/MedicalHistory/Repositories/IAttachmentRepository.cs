using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;

namespace FamilyCare.Application.MedicalHistory.Repositories;

public interface IAttachmentRepository
{
    Task<Attachment?> GetByIdAsync(AttachmentId id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        Guid ownerEntityId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default);

    void Remove(Attachment attachment);
}
