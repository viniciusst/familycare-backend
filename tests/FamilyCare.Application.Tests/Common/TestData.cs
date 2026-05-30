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

    /// <summary>
    /// Builds a family with the given owner plus one accepted member.
    /// Returns the family, owner user id, owner member id, secondary user id, secondary member id.
    /// </summary>
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
}
