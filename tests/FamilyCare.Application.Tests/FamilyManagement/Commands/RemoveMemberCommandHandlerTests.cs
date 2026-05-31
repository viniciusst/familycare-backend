using FamilyCare.Application.FamilyManagement.Commands.RemoveMember;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class RemoveMemberCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private RemoveMemberCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenAdminRemovesOtherMember_ShouldRemove()
    {
        // Arrange
        var (family, ownerUserId, _, _, secondaryMemberId) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new RemoveMemberCommand(family.Id, secondaryMemberId),
            CancellationToken.None);

        // Assert
        Assert.DoesNotContain(family.Members, m => m.Id == secondaryMemberId);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserRemovesSelf_ShouldRemove()
    {
        // Arrange — secondary Adult removes themselves.
        var (family, _, _, secondaryUserId, secondaryMemberId) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(secondaryUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new RemoveMemberCommand(family.Id, secondaryMemberId),
            CancellationToken.None);

        // Assert
        Assert.DoesNotContain(family.Members, m => m.Id == secondaryMemberId);
    }

    [Fact]
    public async Task Handle_WhenNonAdminRemovesOtherMember_ShouldThrowForbidden()
    {
        // Arrange — Adult trying to remove Owner.
        var (family, _, ownerMemberId, adultUserId, _) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adultUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new RemoveMemberCommand(family.Id, ownerMemberId),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTargetMemberNotFound_ShouldThrowNotFound()
    {
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Handle(
            new RemoveMemberCommand(family.Id, FamilyMemberId.New()),
            CancellationToken.None));
    }
}
