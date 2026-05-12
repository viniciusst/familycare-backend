using FamilyCare.Domain.Tests.Common;

namespace FamilyCare.Domain.Tests.FamilyManagement;

public class InvitationTests
{
    [Fact]
    public void InviteMember_ShouldCreatePendingInvitation()
    {
        var family = TestData.AnyFamily();
        var invitedEmail = TestData.AnyEmail("invited");

        var invitation = family.InviteMember(
            invitedEmail, Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7));

        Assert.Equal(invitedEmail, invitation.Email);
        Assert.Equal(Role.Adult, invitation.ProposedRole);
        Assert.Equal(RelationshipType.Sibling, invitation.ProposedRelationship);
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
        Assert.True(invitation.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void InviteMember_WithInvalidTtl_ShouldThrow()
    {
        var family = TestData.AnyFamily();

        Assert.Throws<InvalidEntityStateException>(() => family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Other, TimeSpan.Zero));
    }

    [Fact]
    public void InviteMember_DuplicatePendingEmail_ShouldThrow()
    {
        var family = TestData.AnyFamily();
        var email = TestData.AnyEmail();

        family.InviteMember(email, Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7));

        Assert.Throws<BusinessRuleViolationException>(() => family.InviteMember(
            email, Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7)));
    }

    [Fact]
    public void AcceptInvitation_ShouldAddMemberAndMarkAccepted()
    {
        var family = TestData.AnyFamily();
        var invitation = family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7));

        var newMemberUserId = UserId.New();
        family.AcceptInvitation(invitation.Id, newMemberUserId, "Sibling", new DateOnly(1990, 1, 1));

        Assert.Equal(2, family.Members.Count);
        Assert.Contains(family.Members, m => m.UserId == newMemberUserId);
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
    }

    [Fact]
    public void DeclineInvitation_ShouldMarkDeclined()
    {
        var family = TestData.AnyFamily();
        var invitation = family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        family.DeclineInvitation(invitation.Id);

        Assert.Equal(InvitationStatus.Declined, invitation.Status);
        Assert.Single(family.Members);
    }

    [Fact]
    public void RevokeInvitation_ShouldMarkRevoked()
    {
        var family = TestData.AnyFamily();
        var invitation = family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        family.RevokeInvitation(invitation.Id);

        Assert.Equal(InvitationStatus.Revoked, invitation.Status);
    }
}
