using FamilyCare.Domain.Tests.Common;

namespace FamilyCare.Domain.Tests.FamilyManagement;

public class FamilyTests
{
    [Fact]
    public void Create_ShouldInitializeWithOwnerAsFirstMember()
    {
        var ownerUserId = UserId.New();
        var family = Family.Create(
            "Souza Family",
            ownerUserId,
            "Vinicius",
            new DateOnly(1985, 1, 15));

        Assert.Equal("Souza Family", family.Name);
        Assert.Equal(ownerUserId, family.OwnerUserId);
        Assert.Single(family.Members);

        var owner = family.Members.Single();
        Assert.Equal(ownerUserId, owner.UserId);
        Assert.Equal(Role.Owner, owner.Role);
        Assert.Equal("Vinicius", owner.DisplayName);
    }

    [Fact]
    public void Create_ShouldRaiseFamilyCreatedEvent()
    {
        var family = TestData.AnyFamily();

        Assert.Contains(family.DomainEvents, e => e is FamilyCreatedEvent);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithEmptyName_ShouldThrow(string name)
    {
        Assert.Throws<InvalidEntityStateException>(
            () => Family.Create(name, UserId.New(), "Owner", new DateOnly(1985, 1, 1)));
    }

    [Fact]
    public void Rename_ShouldUpdateName()
    {
        var family = TestData.AnyFamily(name: "Old Name");

        family.Rename("New Name");

        Assert.Equal("New Name", family.Name);
    }

    [Fact]
    public void Rename_WithEmptyName_ShouldThrow()
    {
        var family = TestData.AnyFamily();

        Assert.Throws<InvalidEntityStateException>(() => family.Rename(""));
    }

    [Fact]
    public void TransferOwnership_ToValidMember_ShouldSwapRoles()
    {
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);

        var newOwnerUserId = UserId.New();
        var invitation = family.InviteMember(
            TestData.AnyEmail(),
            Role.Adult,
            RelationshipType.Spouse,
            TimeSpan.FromDays(7));
        family.AcceptInvitation(invitation.Id, newOwnerUserId, "Spouse", new DateOnly(1987, 5, 20));

        var newOwnerMember = family.Members.Single(m => m.UserId == newOwnerUserId);

        family.TransferOwnership(newOwnerMember.Id);

        Assert.Equal(newOwnerUserId, family.OwnerUserId);
        Assert.Equal(Role.Owner, family.Members.Single(m => m.UserId == newOwnerUserId).Role);
        // Previous owner is demoted to Admin (second-highest role), not Adult
        Assert.Equal(Role.Admin, family.Members.Single(m => m.UserId == ownerUserId).Role);
    }

    [Fact]
    public void TransferOwnership_ToNonExistentMember_ShouldThrow()
    {
        var family = TestData.AnyFamily();

        Assert.Throws<EntityNotFoundException>(
            () => family.TransferOwnership(FamilyMemberId.New()));
    }

    [Fact]
    public void RemoveMember_NonOwner_ShouldRemoveMember()
    {
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);
        var invitation = family.InviteMember(
            TestData.AnyEmail(),
            Role.Adult,
            RelationshipType.Sibling,
            TimeSpan.FromDays(7));
        family.AcceptInvitation(invitation.Id, UserId.New(), "Sibling", new DateOnly(1990, 6, 10));

        var memberToRemove = family.Members.Last();

        family.RemoveMember(memberToRemove.Id);

        Assert.DoesNotContain(family.Members, m => m.Id == memberToRemove.Id);
    }

    [Fact]
    public void RemoveMember_OwnerShouldThrow()
    {
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);
        var ownerMemberId = family.Members.Single().Id;

        Assert.Throws<BusinessRuleViolationException>(() => family.RemoveMember(ownerMemberId));
    }
}
