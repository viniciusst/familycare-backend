using FamilyCare.Application.FamilyManagement.Commands.CreateFamily;
using FamilyCare.Application.FamilyManagement.Repositories;

namespace FamilyCare.Application.Tests.FamilyManagement.Commands;

public class CreateFamilyCommandHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IFamilyRepository> _familyRepoMock = new();

    private CreateFamilyCommandHandler CreateSut()
        => new(_currentUserMock.Object, _familyRepoMock.Object);

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateFamilyAndReturnIds()
    {
        // Arrange
        var ownerUserId = UserId.New();
        _currentUserMock.Setup(c => c.RequireUserId()).Returns(ownerUserId);

        var command = new CreateFamilyCommand(
            Name: "Souza Family",
            OwnerDisplayName: "Vinicius",
            OwnerBirthDate: new DateOnly(1985, 1, 15));

        var sut = CreateSut();

        // Act
        var response = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(default, response.FamilyId);
        Assert.NotEqual(default, response.OwnerMemberId);

        _familyRepoMock.Verify(
            r => r.AddAsync(
                It.Is<Family>(f => f.Name == "Souza Family"
                                && f.OwnerUserId == ownerUserId
                                && f.Members.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUnauthenticated_ShouldPropagate()
    {
        // Arrange
        _currentUserMock.Setup(c => c.RequireUserId()).Throws<UnauthenticatedException>();

        var sut = CreateSut();
        var command = new CreateFamilyCommand("Family", "Owner", new DateOnly(1990, 1, 1));

        // Act + Assert
        await Assert.ThrowsAsync<UnauthenticatedException>(
            () => sut.Handle(command, CancellationToken.None));

        _familyRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Family>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class CreateFamilyCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly CreateFamilyCommandValidator _validator;

    public CreateFamilyCommandValidatorTests()
    {
        _clockMock.Setup(c => c.TodayUtc).Returns(new DateOnly(2026, 5, 30));
        _validator = new CreateFamilyCommandValidator(_clockMock.Object);
    }

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new CreateFamilyCommand(
            "Souza Family", "Vinicius", new DateOnly(1985, 1, 15)));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    public void Validate_WithEmptyName_ShouldFail(string name)
    {
        var result = _validator.Validate(new CreateFamilyCommand(
            name, "Vinicius", new DateOnly(1985, 1, 15)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateFamilyCommand.Name));
    }

    [Fact]
    public void Validate_WithFutureBirthDate_ShouldFail()
    {
        var result = _validator.Validate(new CreateFamilyCommand(
            "Family", "Owner", new DateOnly(2027, 1, 1)));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors,
            e => e.PropertyName == nameof(CreateFamilyCommand.OwnerBirthDate));
    }
}
