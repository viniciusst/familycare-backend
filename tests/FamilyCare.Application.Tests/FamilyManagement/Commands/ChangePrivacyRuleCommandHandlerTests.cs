using FamilyCare.Application.FamilyManagement.Commands.ChangePrivacyRule;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class ChangePrivacyRuleCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private ChangePrivacyRuleCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WhenRequesterEditsOwnRule_ShouldUpdate()
    {
        // Arrange — secondary member edits their own rule.
        var (family, _, _, secondaryUserId, secondaryMemberId) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(secondaryUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();
        var command = new ChangePrivacyRuleCommand(
            family.Id, secondaryMemberId,
            DataCategory.MedicalHistory, VisibilityScope.Private, null);

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        var target = family.Members.Single(m => m.Id == secondaryMemberId);
        var rule = target.PrivacyRules.Single(r => r.Category == DataCategory.MedicalHistory);
        Assert.Equal(VisibilityScope.Private, rule.Scope);
        _familyRepoMock.Verify(r => r.Update(family), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAdminEditsOtherMembersRule_ShouldUpdate()
    {
        // Arrange — Owner editing a minor's rules (typical use case).
        var (family, ownerUserId, _, _, secondaryMemberId) =
            TestData.FamilyWithTwoMembers(Role.Minor);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();
        var command = new ChangePrivacyRuleCommand(
            family.Id, secondaryMemberId,
            DataCategory.MedicalHistory, VisibilityScope.FamilyAdmins, null);

        // Act
        await sut.Handle(command, CancellationToken.None);

        // Assert
        var target = family.Members.Single(m => m.Id == secondaryMemberId);
        var rule = target.PrivacyRules.Single(r => r.Category == DataCategory.MedicalHistory);
        Assert.Equal(VisibilityScope.FamilyAdmins, rule.Scope);
    }

    [Fact]
    public async Task Handle_WhenNonAdminEditsOtherMembersRule_ShouldThrowForbidden()
    {
        // Arrange — Adult trying to edit owner's privacy rules.
        var (family, _, ownerMemberId, adultUserId, _) =
            TestData.FamilyWithTwoMembers(Role.Adult);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(adultUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();
        var command = new ChangePrivacyRuleCommand(
            family.Id, ownerMemberId,
            DataCategory.MedicalHistory, VisibilityScope.Private, null);

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WhenTargetMemberNotFound_ShouldThrowNotFound()
    {
        // Arrange
        var ownerUserId = UserId.New();
        var family = TestData.AnyFamily(ownerUserId: ownerUserId);

        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);
        _familyRepoMock
            .Setup(r => r.GetByIdAsync(family.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(family);

        var sut = CreateSut();
        var command = new ChangePrivacyRuleCommand(
            family.Id, FamilyMemberId.New(),
            DataCategory.MedicalHistory, VisibilityScope.Private, null);

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(command, CancellationToken.None));
    }
}

public class ChangePrivacyRuleCommandValidatorTests
{
    private readonly ChangePrivacyRuleCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidNonCustomInput_ShouldPass()
    {
        var result = _validator.Validate(new ChangePrivacyRuleCommand(
            FamilyId.New(), FamilyMemberId.New(),
            DataCategory.MedicalHistory, VisibilityScope.Private, null));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithCustomScopeAndAllowedIds_ShouldPass()
    {
        var result = _validator.Validate(new ChangePrivacyRuleCommand(
            FamilyId.New(), FamilyMemberId.New(),
            DataCategory.MedicalHistory, VisibilityScope.Custom,
            new[] { FamilyMemberId.New() }));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithCustomScopeButNoAllowedIds_ShouldFail()
    {
        var result = _validator.Validate(new ChangePrivacyRuleCommand(
            FamilyId.New(), FamilyMemberId.New(),
            DataCategory.MedicalHistory, VisibilityScope.Custom, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithNonCustomScopeButAllowedIdsProvided_ShouldFail()
    {
        var result = _validator.Validate(new ChangePrivacyRuleCommand(
            FamilyId.New(), FamilyMemberId.New(),
            DataCategory.MedicalHistory, VisibilityScope.Private,
            new[] { FamilyMemberId.New() }));

        Assert.False(result.IsValid);
    }
}
