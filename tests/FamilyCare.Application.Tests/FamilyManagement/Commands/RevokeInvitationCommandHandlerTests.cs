using FamilyCare.Application.FamilyManagement.Commands.RevokeInvitation;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class RevokeInvitationCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private RevokeInvitationCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsAdmin_ShouldRevokeAndPersist()
    {
        // Arrange
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);
        var invitation = family.InviteMember(
            TestData.AnyEmail(), Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new RevokeInvitationCommand(family.Id, invitation.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(InvitationStatus.Revoked, invitation.Status);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotAdmin_ShouldThrowForbidden()
    {
        // Arrange
        var (family, _, _, adultUserId, _) = TestData.FamilyWithTwoMembers(Role.Adult);
        var invitation = family.InviteMember(
            TestData.AnyEmail("new"), Role.Adult, RelationshipType.Other, TimeSpan.FromDays(7));

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adultUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new RevokeInvitationCommand(family.Id, invitation.Id),
            CancellationToken.None));
    }
}
