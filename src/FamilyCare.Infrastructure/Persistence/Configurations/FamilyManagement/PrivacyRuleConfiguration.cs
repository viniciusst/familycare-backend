using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FamilyCare.Infrastructure.Persistence.Configurations.FamilyManagement;

public sealed class PrivacyRuleConfiguration : IEntityTypeConfiguration<PrivacyRule>
{
    public void Configure(EntityTypeBuilder<PrivacyRule> builder)
    {
        builder.ToTable("privacy_rules");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion<PrivacyRuleIdConverter>()
            .ValueGeneratedNever();

        builder.Property(r => r.FamilyMemberId)
            .HasConversion<FamilyMemberIdConverter>()
            .IsRequired();

        builder.Property(r => r.Category)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Scope)
            .HasConversion<int>()
            .IsRequired();

        // Map IReadOnlyCollection<FamilyMemberId> as a Postgres uuid[] column.
        // The internal field is a HashSet<FamilyMemberId>, so the converter must
        // produce a HashSet on read — returning a List causes an InvalidCastException
        // when EF Core assigns the value to the backing field via reflection.
        var converter = new ValueConverter<IReadOnlyCollection<FamilyMemberId>, Guid[]>(
            ids => ids.Select(id => id.Value).ToArray(),
            arr => new HashSet<FamilyMemberId>(arr.Select(g => FamilyMemberId.From(g))));

        var comparer = new ValueComparer<IReadOnlyCollection<FamilyMemberId>>(
            (a, b) => (a ?? Array.Empty<FamilyMemberId>())
                .SequenceEqual(b ?? Array.Empty<FamilyMemberId>()),
            v => v.Aggregate(0, (h, id) => HashCode.Combine(h, id.GetHashCode())),
            v => (IReadOnlyCollection<FamilyMemberId>)new HashSet<FamilyMemberId>(v));

        builder.Property(r => r.AllowedMemberIds)
            .HasField("_allowedMemberIds")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion(converter, comparer)
            .HasColumnType("uuid[]")
            .IsRequired();

        builder.HasIndex(r => new { r.FamilyMemberId, r.Category }).IsUnique();
    }
}
