using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;

public sealed class AttachmentRepository(FamilyCareDbContext dbContext) : IAttachmentRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Attachment?> GetByIdAsync(AttachmentId id, CancellationToken cancellationToken = default)
        => _dbContext.Attachments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Attachment>> GetByOwnerAsync(
        AttachmentOwnerType ownerType,
        Guid ownerEntityId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Attachments
            .Where(a => a.OwnerType == ownerType && a.OwnerEntityId == ownerEntityId)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Attachment attachment, CancellationToken cancellationToken = default)
        => await _dbContext.Attachments.AddAsync(attachment, cancellationToken);

    public void Remove(Attachment attachment) => _dbContext.Attachments.Remove(attachment);
}
