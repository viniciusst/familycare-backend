using FamilyCare.Application.FamilyManagement.Commands.TransferOwnership;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class TransferOwnershipCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private TransferOwnershipCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsOwner_ShouldTransferOwnership()
    {
        // Arrange
        var (family, ownerUserId, _, secondaryUserId, secondaryMemberId) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new TransferOwnershipCommand(family.Id, secondaryMemberId),
            CancellationToken.None);

        // Assert
        Assert.Equal(secondaryUserId, family.OwnerUserId);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsAdminButNotOwner_ShouldThrowForbidden()
    {
        // Arrange — admin can't transfer ownership.
        var (family, _, ownerMemberId, adminUserId, _) =
            TestData.FamilyWithTwoMembers(Role.Admin);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adminUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new TransferOwnershipCommand(family.Id, ownerMemberId),
            CancellationToken.None));
    }
}
