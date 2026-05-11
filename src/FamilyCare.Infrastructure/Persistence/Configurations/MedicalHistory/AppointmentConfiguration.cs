using FamilyCare.Domain.MedicalHistory;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.MedicalHistory;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .HasConversion<AppointmentIdConverter>()
            .ValueGeneratedNever();

        builder.Property(a => a.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(a => a.ScheduledAt).IsRequired();
        builder.Property(a => a.Specialty).HasMaxLength(80).IsRequired();
        builder.Property(a => a.DoctorName).HasMaxLength(120).IsRequired();
        builder.Property(a => a.Location).HasMaxLength(200);
        builder.Property(a => a.Notes).HasMaxLength(2000);
        builder.Property(a => a.Status).HasConversion<int>().IsRequired();

        builder.HasIndex(a => new { a.FamilyMemberId, a.ScheduledAt });
        builder.HasIndex(a => a.Status);

        builder.Ignore(a => a.DomainEvents);
    }
}
