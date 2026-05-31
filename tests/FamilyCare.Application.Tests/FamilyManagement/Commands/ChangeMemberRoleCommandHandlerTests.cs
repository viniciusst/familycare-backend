using FamilyCare.Application.FamilyManagement.Commands.ChangeMemberRole;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class ChangeMemberRoleCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private ChangeMemberRoleCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsOwner_ShouldChangeRole()
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
            new ChangeMemberRoleCommand(family.Id, secondaryMemberId, Role.Admin),
            CancellationToken.None);

        // Assert
        var updated = family.Members.Single(m => m.Id == secondaryMemberId);
        Assert.Equal(Role.Admin, updated.Role);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequesterIsAdminButNotOwner_ShouldThrowForbidden()
    {
        // Arrange — secondary user is Admin (not Owner).
        var (family, _, _, adminUserId, ownerMemberId) =
            TestData.FamilyWithTwoMembers(Role.Admin);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adminUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new ChangeMemberRoleCommand(family.Id, ownerMemberId, Role.Adult),
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotMember_ShouldThrowForbidden()
    {
        var family = TestData.AnyFamily();
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();

        await Assert.ThrowsAsync<ForbiddenException>(() => sut.Handle(
            new ChangeMemberRoleCommand(family.Id, FamilyMemberId.New(), Role.Admin),
            CancellationToken.None));
    }
}

public class ChangeMemberRoleCommandValidatorTests
{
    private readonly ChangeMemberRoleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidRole_ShouldPass()
    {
        var result = _validator.Validate(new ChangeMemberRoleCommand(
            FamilyId.New(), FamilyMemberId.New(), Role.Admin));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithUndefinedRole_ShouldFail()
    {
        var result = _validator.Validate(new ChangeMemberRoleCommand(
            FamilyId.New(), FamilyMemberId.New(), (Role)999));

        Assert.False(result.IsValid);
    }
}
