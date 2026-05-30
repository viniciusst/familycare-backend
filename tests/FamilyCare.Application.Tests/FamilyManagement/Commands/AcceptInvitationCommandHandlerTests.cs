using FamilyCare.Application.FamilyManagement.Commands.AcceptInvitation;
using FamilyCare.Application.FamilyManagement.Commands.DeclineInvitation;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class AcceptInvitationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();

    private AcceptInvitationCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object, _userRepoMock.Object);

    private static (Family Family, Invitation Invitation, Email InvitedEmail) BuildPendingInvitation()
    {
        var family = TestData.AnyFamily();
        var invitedEmail = TestData.AnyEmail("invited");
        var invitation = family.InviteMember(
            invitedEmail, Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7));
        return (family, invitation, invitedEmail);
    }

    [Fact]
    public async Task Handle_WhenUserMatchesInvitedEmail_ShouldAcceptAndPersist()
    {
        // Arrange
        var (family, invitation, invitedEmail) = BuildPendingInvitation();
        var invitedUser = TestData.AnyUser(email: invitedEmail);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(invitedUser.Id);
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((family, invitation));
        _userRepoMock
            .Setup(r => r.GetByIdAsync(invitedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitedUser);

        var sut = CreateSut();
        var command = new AcceptInvitationCommand(
            invitation.Id, "Sibling", new DateOnly(1992, 6, 5));

        // Act
        var response = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(family.Id, response.FamilyId);
        Assert.NotEqual(default, response.MemberId);
        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvitationNotFound_ShouldThrowNotFound()
    {
        // Arrange
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(
                It.IsAny<InvitationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(((Family, Invitation)?)null);

        var sut = CreateSut();
        var command = new AcceptInvitationCommand(
            InvitationId.New(), "Name", new DateOnly(1990, 1, 1));

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenUserEmailDoesNotMatchInvitation_ShouldThrowForbidden()
    {
        // Arrange — invitation was for a different email.
        var (family, invitation, _) = BuildPendingInvitation();
        var otherUser = TestData.AnyUser(email: TestData.AnyEmail("other"));

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(otherUser.Id);
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((family, invitation));
        _userRepoMock
            .Setup(r => r.GetByIdAsync(otherUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);

        var sut = CreateSut();
        var command = new AcceptInvitationCommand(
            invitation.Id, "Name", new DateOnly(1990, 1, 1));

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
        Assert.Equal(InvitationStatus.Pending, invitation.Status);
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyMember_ShouldThrowConflict()
    {
        // Arrange — already-member scenario:
        // user is in family AND has a pending invitation matching their email.
        var (family, ownerUserId, _, secondaryUserId, _) = TestData.FamilyWithTwoMembers();
        var secondaryUser = TestData.AnyUser();
        // Trick: build a separate invitation with secondaryUser's email
        var invitation = family.InviteMember(
            secondaryUser.Email, Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        // Forge: pretend the secondaryUser matches the invitation email exactly
        // (We use a fresh secondaryUser with the same email used in the invitation.)
        var matchingUser = TestData.AnyUser(email: invitation.Email);
        // Add them as if they were already in: we leverage the existing two-member family
        // and use the matchingUser's id which is NOT in family. So we need to ensure
        // the test arrangement creates an already-existing membership AND a matching invitation.
        //
        // Simpler approach: use the existing secondaryUserId (already a member)
        // and a separate invitation that targets that same email.
        var existingMember = family.Members.Single(m => m.UserId == secondaryUserId);
        var alreadyMemberUser = User.Register(
            email: TestData.AnyEmail("already-member"),
            passwordHash: TestData.AnyPasswordHash(),
            preferredLanguage: SupportedLanguage.EnglishCanada);
        var dupeInvitation = family.InviteMember(
            alreadyMemberUser.Email, Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        // Add alreadyMemberUser as a member through a separate path is not possible in domain
        // without going through invitation. So we use the existing secondaryUserId + craft
        // a user instance whose Id equals secondaryUserId but with email matching dupeInvitation.
        //
        // Since UserId is a strong-typed struct, we can't easily mismatch. Test pivot:
        // verify that when user.Email matches the invitation but is already in family,
        // ConflictException is thrown.
        var conflictUser = TestData.AnyUser(email: dupeInvitation.Email);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(secondaryUserId);
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(dupeInvitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((family, dupeInvitation));
        _userRepoMock
            .Setup(r => r.GetByIdAsync(secondaryUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictUser);

        // Note: The conflictUser has the email matching dupeInvitation, so the email-match
        // check passes. The "already member" check uses UserId (secondaryUserId IS in family).
        var sut = CreateSut();
        var command = new AcceptInvitationCommand(
            dupeInvitation.Id, "Name", new DateOnly(1990, 1, 1));

        // Act + Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => sut.Handle(command, CancellationToken.None));
    }
}

public class AcceptInvitationCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly AcceptInvitationCommandValidator _validator;

    public AcceptInvitationCommandValidatorTests()
    {
        _clockMock.Setup(c => c.TodayUtc).Returns(new DateOnly(2026, 5, 30));
        _validator = new AcceptInvitationCommandValidator(_clockMock.Object);
    }

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new AcceptInvitationCommand(
            InvitationId.New(), "Sibling", new DateOnly(1992, 1, 1)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithFutureBirthDate_ShouldFail()
    {
        var result = _validator.Validate(new AcceptInvitationCommand(
            InvitationId.New(), "Sibling", new DateOnly(2027, 1, 1)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyDisplayName_ShouldFail()
    {
        var result = _validator.Validate(new AcceptInvitationCommand(
            InvitationId.New(), "", new DateOnly(1990, 1, 1)));

        Assert.False(result.IsValid);
    }
}

public class DeclineInvitationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();

    private DeclineInvitationCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object, _userRepoMock.Object);

    [Fact]
    public async Task Handle_WhenUserMatchesInvitation_ShouldDeclineAndPersist()
    {
        // Arrange
        var family = TestData.AnyFamily();
        var invitedEmail = TestData.AnyEmail("invited");
        var invitation = family.InviteMember(
            invitedEmail, Role.Adult, RelationshipType.Sibling, TimeSpan.FromDays(7));
        var invitedUser = TestData.AnyUser(email: invitedEmail);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(invitedUser.Id);
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((family, invitation));
        _userRepoMock
            .Setup(r => r.GetByIdAsync(invitedUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invitedUser);

        var sut = CreateSut();

        // Act
        await sut.Handle(new DeclineInvitationCommand(invitation.Id), CancellationToken.None);

        // Assert
        Assert.Equal(InvitationStatus.Declined, invitation.Status);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailMismatch_ShouldThrowForbidden()
    {
        // Arrange
        var family = TestData.AnyFamily();
        var invitation = family.InviteMember(
            TestData.AnyEmail("invited"), Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));
        var otherUser = TestData.AnyUser(email: TestData.AnyEmail("other"));

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(otherUser.Id);
        _familyRepoMock
            .Setup(r => r.FindPendingInvitationByIdAsync(invitation.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((family, invitation));
        _userRepoMock
            .Setup(r => r.GetByIdAsync(otherUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUser);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new DeclineInvitationCommand(invitation.Id), CancellationToken.None));
    }
}
