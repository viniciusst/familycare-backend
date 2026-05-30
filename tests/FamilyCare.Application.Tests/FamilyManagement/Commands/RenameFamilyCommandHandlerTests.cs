using FamilyCare.Application.FamilyManagement.Commands.RenameFamily;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class RenameFamilyCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private RenameFamilyCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsOwner_ShouldRenameFamily()
    {
        // Arrange
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId, name: "Old Name");

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act
        await sut.Handle(new RenameFamilyCommand(family.Id, "New Name"), CancellationToken.None);

        // Assert
        Assert.Equal("New Name", family.Name);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenFamilyNotFound_ShouldThrowNotFound()
    {
        // Arrange
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<FamilyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Family?)null);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(new RenameFamilyCommand(FamilyId.New(), "Name"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotMember_ShouldThrowForbidden()
    {
        // Arrange
        var family = TestData.AnyFamily();
        var stranger = UserId.New();

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(stranger);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new RenameFamilyCommand(family.Id, "Name"), CancellationToken.None));
        _familyRepoMock.Verify(r => r.Update(It.IsAny<Family>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotAdmin_ShouldThrowForbidden()
    {
        // Arrange — Adult member trying to rename.
        var (family, _, _, adultUserId, _) = TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adultUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(new RenameFamilyCommand(family.Id, "Name"), CancellationToken.None));
    }
}

public class RenameFamilyCommandValidatorTests
{
    private readonly RenameFamilyCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidName_ShouldPass()
    {
        var result = _validator.Validate(new RenameFamilyCommand(FamilyId.New(), "New Name"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        var result = _validator.Validate(new RenameFamilyCommand(FamilyId.New(), ""));

        Assert.False(result.IsValid);
    }
}
