namespace FamilyCare.Application.Tests.Common;

internal static class TestData
{
    public static Email AnyEmail(string? local = null)
        => Email.Create($"{local ?? Guid.NewGuid().ToString("N")[..8]}@familycare.test");

    public static PasswordHash AnyPasswordHash()
        => PasswordHash.FromHashed("$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQR");

    public static User AnyUser(
        Email? email = null,
        SupportedLanguage language = SupportedLanguage.EnglishCanada)
        => User.Register(
            email: email ?? AnyEmail(),
            passwordHash: AnyPasswordHash(),
            preferredLanguage: language);

    public static Family AnyFamily(
        UserId? ownerUserId = null,
        string name = "Test Family",
        string ownerDisplayName = "Owner",
        DateOnly? ownerBirthDate = null)
        => Family.Create(
            name: name,
            ownerUserId: ownerUserId ?? UserId.New(),
            ownerDisplayName: ownerDisplayName,
            ownerBirthDate: ownerBirthDate ?? new DateOnly(1985, 1, 15));

    public static (Family Family, UserId OwnerUserId, FamilyMemberId OwnerMemberId,
                   UserId SecondaryUserId, FamilyMemberId SecondaryMemberId)
        FamilyWithTwoMembers(Role secondaryRole = Role.Adult)
    {
        var ownerUserId = UserId.New();
        var family = AnyFamily(ownerUserId: ownerUserId);
        var ownerMemberId = family.Members.Single().Id;

        var invitation = family.InviteMember(
            AnyEmail(), secondaryRole, RelationshipType.Spouse, TimeSpan.FromDays(7));
        var secondaryUserId = UserId.New();
        family.AcceptInvitation(invitation.Id, secondaryUserId, "Secondary", new DateOnly(1988, 3, 10));
        var secondaryMemberId = family.Members.Single(m => m.UserId == secondaryUserId).Id;

        return (family, ownerUserId, ownerMemberId, secondaryUserId, secondaryMemberId);
    }

    // ---- Medical entity factories ----

    public static Appointment AnyAppointment(FamilyMemberId? memberId = null)
        => Appointment.Schedule(
            memberId ?? FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Cardiology",
            "Dr. House",
            "Clinic A",
            null);

    public static Exam AnyExam(FamilyMemberId? memberId = null)
        => Exam.Register(
            memberId ?? FamilyMemberId.New(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Blood Test",
            "LabCorp",
            "Initial results",
            "Dr. Smith");

    public static Vaccine AnyVaccine(FamilyMemberId? memberId = null)
        => Vaccine.Register(
            memberId ?? FamilyMemberId.New(),
            "COVID-19 Pfizer",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            "Pfizer", "ABC123", 2, null, null);

    public static Allergy AnyAllergy(
        FamilyMemberId? memberId = null,
        AllergySeverity severity = AllergySeverity.Mild)
        => Allergy.Register(
            memberId ?? FamilyMemberId.New(),
            "Peanuts",
            severity,
            "Hives",
            new DateOnly(2010, 5, 15));

    public static ChronicCondition AnyChronicCondition(FamilyMemberId? memberId = null)
        => ChronicCondition.Register(
            memberId ?? FamilyMemberId.New(),
            "Hypertension",
            new DateOnly(2020, 1, 1),
            "Mild");
}
