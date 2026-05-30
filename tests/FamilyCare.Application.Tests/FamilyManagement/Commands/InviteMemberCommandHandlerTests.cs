using FamilyCare.Application.FamilyManagement.Commands.InviteMember;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class InviteMemberCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private InviteMemberCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterIsOwner_ShouldInviteAndPersist()
    {
        // Arrange
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var command = new InviteMemberCommand(
            family.Id,
            "newmember@familycare.test",
            Role.Adult,
            RelationshipType.Sibling,
            TtlDays: 14);

        var sut = CreateSut();

        // Act
        var response = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(default, response.InvitationId);
        Assert.True(response.ExpiresAt > DateTime.UtcNow);
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
        var command = new InviteMemberCommand(
            FamilyId.New(), "x@y.test", Role.Adult, RelationshipType.Other);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(command, CancellationToken.None));
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
        var command = new InviteMemberCommand(
            family.Id, "x@y.test", Role.Adult, RelationshipType.Other);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenRequesterIsNotAdmin_ShouldThrowForbidden()
    {
        // Arrange — Adult cannot invite.
        var (family, _, _, adultUserId, _) = TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adultUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();
        var command = new InviteMemberCommand(
            family.Id, "x@y.test", Role.Adult, RelationshipType.Other);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
        _familyRepoMock.Verify(r => r.Update(It.IsAny<Family>()), Times.Never);
    }
}

public class InviteMemberCommandValidatorTests
{
    private readonly InviteMemberCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new InviteMemberCommand(
            FamilyId.New(), "user@test.com", Role.Adult, RelationshipType.Sibling, 7));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(31)]
    [InlineData(-1)]
    public void Validate_WithTtlOutOfRange_ShouldFail(int ttlDays)
    {
        var result = _validator.Validate(new InviteMemberCommand(
            FamilyId.New(), "user@test.com", Role.Adult, RelationshipType.Sibling, ttlDays));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(InviteMemberCommand.TtlDays));
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        var result = _validator.Validate(new InviteMemberCommand(
            FamilyId.New(), "", Role.Adult, RelationshipType.Sibling));

        Assert.False(result.IsValid);
    }
}
