using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class AllergyConfiguration : IEntityTypeConfiguration<Allergy>
{
    public void Configure(EntityTypeBuilder<Allergy> builder)
    {
        builder.ToTable("allergies");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<AllergyIdConverter>()
            .ValueGeneratedNever();

        builder.Property(a => a.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(a => a.Substance).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Severity).HasConversion<int>().IsRequired();
        builder.Property(a => a.Reaction).HasMaxLength(2000);
        builder.Property(a => a.FirstObservedAt);

        builder.HasIndex(a => a.FamilyMemberId);

        builder.Ignore(a => a.DomainEvents);
    }
}
