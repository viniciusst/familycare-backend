using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class VaccineConfiguration : IEntityTypeConfiguration<Vaccine>
{
    public void Configure(EntityTypeBuilder<Vaccine> builder)
    {
        builder.ToTable("vaccines");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasConversion<VaccineIdConverter>()
            .ValueGeneratedNever();

        builder.Property(v => v.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(v => v.Name).HasMaxLength(120).IsRequired();
        builder.Property(v => v.AppliedAt).IsRequired();
        builder.Property(v => v.Manufacturer).HasMaxLength(120);
        builder.Property(v => v.BatchNumber).HasMaxLength(80);
        builder.Property(v => v.DoseNumber);
        builder.Property(v => v.NextDoseDue);
        builder.Property(v => v.Notes).HasMaxLength(2000);

        builder.HasIndex(v => new { v.FamilyMemberId, v.AppliedAt });
        builder.HasIndex(v => v.NextDoseDue);

        builder.Ignore(v => v.DomainEvents);
    }
}
