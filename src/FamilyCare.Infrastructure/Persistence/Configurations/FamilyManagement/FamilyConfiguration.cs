using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.FamilyManagement;

public sealed class FamilyConfiguration : IEntityTypeConfiguration<Family>
{
    public void Configure(EntityTypeBuilder<Family> builder)
    {
        builder.ToTable("families");

        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasConversion<FamilyIdConverter>()
            .ValueGeneratedNever();

        builder.Property(f => f.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(f => f.OwnerUserId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(f => f.CreatedAt).IsRequired();

        builder.HasIndex(f => f.OwnerUserId);

        // Members owned by this aggregate (cascade delete on family removal)
        builder.HasMany(f => f.Members)
            .WithOne()
            .HasForeignKey(m => m.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_members");

        // Invitations also part of the aggregate
        builder.HasMany(f => f.Invitations)
            .WithOne()
            .HasForeignKey(i => i.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.Invitations)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_invitations");

        builder.Ignore(f => f.DomainEvents);
    }
}
