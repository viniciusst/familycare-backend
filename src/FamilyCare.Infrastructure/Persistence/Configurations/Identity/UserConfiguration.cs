using FamilyCare.Domain.Identity;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FamilyCare.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion<UserIdConverter>()
            .ValueGeneratedNever();

        // Email VO mapped as a single column with its Value
        builder.Property(u => u.Email)
            .HasConversion(
                e => e.Value,
                v => Email.Create(v))
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email).IsUnique();

        // PasswordHash VO mapped as a single column
        builder.Property(u => u.PasswordHash)
            .HasConversion(
                p => p.Value,
                v => PasswordHash.FromHashed(v))
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.PreferredLanguage)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.CreatedAt).IsRequired();

        // Ignore domain events accumulator (handled by dispatcher)
        builder.Ignore(u => u.DomainEvents);
    }
}
