using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.ToTable("exams");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion<ExamIdConverter>()
            .ValueGeneratedNever();

        builder.Property(e => e.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(e => e.ExamDate).IsRequired();
        builder.Property(e => e.ExamType).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Laboratory).HasMaxLength(120);
        builder.Property(e => e.Results).HasMaxLength(8000);
        builder.Property(e => e.RequestedBy).HasMaxLength(120);

        builder.HasIndex(e => new { e.FamilyMemberId, e.ExamDate });

        builder.Ignore(e => e.DomainEvents);
    }
}
