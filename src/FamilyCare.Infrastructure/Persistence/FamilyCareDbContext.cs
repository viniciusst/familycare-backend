using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using FamilyCare.Domain.MedicalHistory;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence;

public sealed class FamilyCareDbContext(DbContextOptions<FamilyCareDbContext> options)
    : DbContext(options)
{
    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // FamilyManagement
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<PrivacyRule> PrivacyRules => Set<PrivacyRule>();

    // MedicalHistory
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<Vaccine> Vaccines => Set<Vaccine>();
    public DbSet<Allergy> Allergies => Set<Allergy>();
    public DbSet<ChronicCondition> ChronicConditions => Set<ChronicCondition>();
    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all IEntityTypeConfiguration<T> in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FamilyCareDbContext).Assembly);
    }
}
