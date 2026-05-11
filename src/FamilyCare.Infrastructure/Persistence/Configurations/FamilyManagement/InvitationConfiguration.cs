using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.FamilyManagement;

public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion<InvitationIdConverter>()
            .ValueGeneratedNever();

        builder.Property(i => i.FamilyId)
            .HasConversion<FamilyIdConverter>()
            .IsRequired();

        builder.Property(i => i.Email)
            .HasConversion(
                e => e.Value,
                v => Email.Create(v))
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(i => i.ProposedRole)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.ProposedRelationship)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(i => i.CreatedAt).IsRequired();
        builder.Property(i => i.ExpiresAt).IsRequired();
        builder.Property(i => i.RespondedAt);

        builder.HasIndex(i => new { i.FamilyId, i.Status });
        builder.HasIndex(i => i.Email);
    }
}
