using FamilyCare.Domain.Tests.Common;

namespace FamilyCare.Domain.Tests.FamilyManagement;

public class PrivacyRuleTests
{
    private static (Family family, FamilyMemberId ownerMemberId,
                    UserId otherUserId, FamilyMemberId otherMemberId) BuildFamilyWithTwoMembers()
    {
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);
        var ownerMemberId = family.Members.Single().Id;

        var invitation = family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Spouse, TimeSpan.FromDays(7));
        var otherUserId = UserId.New();
        family.AcceptInvitation(invitation.Id, otherUserId, "Spouse", new DateOnly(1987, 5, 20));
        var otherMemberId = family.Members.Single(m => m.UserId == otherUserId).Id;

        return (family, ownerMemberId, otherUserId, otherMemberId);
    }

    [Fact]
    public void ChangePrivacyRule_ToPrivate_ShouldUpdateScope()
    {
        var (family, ownerMemberId, _, _) = BuildFamilyWithTwoMembers();

        family.ChangePrivacyRule(
            ownerMemberId, DataCategory.MedicalHistory, VisibilityScope.Private, null);

        var owner = family.Members.Single(m => m.Id == ownerMemberId);
        var rule = owner.PrivacyRules.Single(r => r.Category == DataCategory.MedicalHistory);
        Assert.Equal(VisibilityScope.Private, rule.Scope);
    }

    [Fact]
    public void ChangePrivacyRule_ToFamilyAdmins_ShouldUpdateScope()
    {
        var (family, ownerMemberId, _, _) = BuildFamilyWithTwoMembers();

        family.ChangePrivacyRule(
            ownerMemberId, DataCategory.Medications, VisibilityScope.FamilyAdmins, null);

        var owner = family.Members.Single(m => m.Id == ownerMemberId);
        var rule = owner.PrivacyRules.Single(r => r.Category == DataCategory.Medications);
        Assert.Equal(VisibilityScope.FamilyAdmins, rule.Scope);
    }

    [Fact]
    public void ChangePrivacyRule_ToCustom_ShouldStoreAllowedMembers()
    {
        var (family, ownerMemberId, _, otherMemberId) = BuildFamilyWithTwoMembers();

        family.ChangePrivacyRule(
            ownerMemberId,
            DataCategory.MedicalHistory,
            VisibilityScope.Custom,
            new[] { otherMemberId });

        var owner = family.Members.Single(m => m.Id == ownerMemberId);
        var rule = owner.PrivacyRules.Single(r => r.Category == DataCategory.MedicalHistory);
        Assert.Equal(VisibilityScope.Custom, rule.Scope);
        Assert.Single(rule.AllowedMemberIds, otherMemberId);
    }

    [Fact]
    public void ChangePrivacyRule_CustomWithUnknownMember_ShouldThrow()
    {
        var (family, ownerMemberId, _, _) = BuildFamilyWithTwoMembers();

        Assert.Throws<BusinessRuleViolationException>(() => family.ChangePrivacyRule(
            ownerMemberId,
            DataCategory.MedicalHistory,
            VisibilityScope.Custom,
            new[] { FamilyMemberId.New() }));
    }

    [Fact]
    public void ChangePrivacyRule_ForUnknownMember_ShouldThrow()
    {
        var (family, _, _, _) = BuildFamilyWithTwoMembers();

        Assert.Throws<EntityNotFoundException>(() => family.ChangePrivacyRule(
            FamilyMemberId.New(),
            DataCategory.MedicalHistory,
            VisibilityScope.Private,
            null));
    }
}
