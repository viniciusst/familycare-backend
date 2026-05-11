using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<AttachmentIdConverter>()
            .ValueGeneratedNever();

        builder.Property(a => a.OwnerEntityId).IsRequired();

        builder.Property(a => a.OwnerType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.FileName).HasMaxLength(255).IsRequired();
        builder.Property(a => a.MimeType).HasMaxLength(127).IsRequired();
        builder.Property(a => a.StoragePath).HasMaxLength(500).IsRequired();
        builder.Property(a => a.SizeBytes).IsRequired();

        builder.Property(a => a.UploadedByMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(a => a.UploadedAt).IsRequired();

        // Index for "all attachments of this owner" lookups
        builder.HasIndex(a => new { a.OwnerType, a.OwnerEntityId });

        builder.Ignore(a => a.DomainEvents);
    }
}
