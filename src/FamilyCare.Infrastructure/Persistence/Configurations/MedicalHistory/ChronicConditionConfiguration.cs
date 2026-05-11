using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class ChronicConditionConfiguration : IEntityTypeConfiguration<ChronicCondition>
{
    public void Configure(EntityTypeBuilder<ChronicCondition> builder)
    {
        builder.ToTable("chronic_conditions");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion<ChronicConditionIdConverter>()
            .ValueGeneratedNever();

        builder.Property(c => c.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(c => c.Name).HasMaxLength(120).IsRequired();
        builder.Property(c => c.DiagnosedAt).IsRequired();
        builder.Property(c => c.Notes).HasMaxLength(4000);
        builder.Property(c => c.IsActive).IsRequired();

        builder.HasIndex(c => new { c.FamilyMemberId, c.IsActive });

        builder.Ignore(c => c.DomainEvents);
    }
}
