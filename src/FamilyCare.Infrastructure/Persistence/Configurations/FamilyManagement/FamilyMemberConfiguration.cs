using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.FamilyManagement;

public sealed class FamilyMemberConfiguration : IEntityTypeConfiguration<FamilyMember>
{
    public void Configure(EntityTypeBuilder<FamilyMember> builder)
    {
        builder.ToTable("family_members");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion<FamilyMemberIdConverter>()
            .ValueGeneratedNever();

        builder.Property(m => m.FamilyId)
            .HasConversion<FamilyIdConverter>()
            .IsRequired();

        builder.Property(m => m.UserId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(m => m.DisplayName)
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(m => m.BirthDate).IsRequired();

        builder.Property(m => m.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.Relationship)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.JoinedAt).IsRequired();

        builder.HasIndex(m => new { m.FamilyId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);

        // PrivacyRules owned by this member
        builder.HasMany(m => m.PrivacyRules)
            .WithOne()
            .HasForeignKey(r => r.FamilyMemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.PrivacyRules)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_privacyRules");
    }
}
