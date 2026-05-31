using FamilyCare.Application.MedicalHistory.Commands.ChangeAllergySeverity;
using FamilyCare.Application.MedicalHistory.Commands.RegisterAllergy;
using FamilyCare.Application.MedicalHistory.Commands.RegisterChronicCondition;
using FamilyCare.Application.MedicalHistory.Commands.ResolveChronicCondition;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.MedicalHistory.Commands;

public class RegisterAllergyCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAllergyRepository> _repoMock = new();

    private RegisterAllergyCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldRegisterAndPersist()
    {
        _guard.WithAllAccessGranted();
        var sut = CreateSut();

        var command = new RegisterAllergyCommand(
            FamilyMemberId.New(),
            "Peanuts",
            AllergySeverity.Severe,
            "Hives");

        var allergyId = await sut.Handle(command, CancellationToken.None);

        Assert.NotEqual(default, allergyId);
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<Allergy>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class ChangeAllergySeverityCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAllergyRepository> _repoMock = new();

    private ChangeAllergySeverityCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldUpdateSeverity()
    {
        var allergy = Allergy.Register(
            FamilyMemberId.New(), "Peanuts", AllergySeverity.Mild);

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(allergy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(allergy);

        var sut = CreateSut();

        await sut.Handle(
            new ChangeAllergySeverityCommand(allergy.Id, AllergySeverity.Severe),
            CancellationToken.None);

        Assert.Equal(AllergySeverity.Severe, allergy.Severity);
        _repoMock.Verify(r => r.Update(allergy), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AllergyId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Allergy?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Handle(
            new ChangeAllergySeverityCommand(AllergyId.New(), AllergySeverity.Severe),
            CancellationToken.None));
    }
}

public class RegisterChronicConditionCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IChronicConditionRepository> _repoMock = new();

    private RegisterChronicConditionCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldRegisterAndPersist()
    {
        _guard.WithAllAccessGranted();
        var sut = CreateSut();

        var command = new RegisterChronicConditionCommand(
            FamilyMemberId.New(),
            "Hypertension",
            new DateOnly(2020, 1, 1));

        var id = await sut.Handle(command, CancellationToken.None);

        Assert.NotEqual(default, id);
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<ChronicCondition>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class ResolveChronicConditionCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IChronicConditionRepository> _repoMock = new();

    private ResolveChronicConditionCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldMarkResolved()
    {
        var condition = ChronicCondition.Register(
            FamilyMemberId.New(),
            "Hypertension",
            new DateOnly(2020, 1, 1));

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(condition.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(condition);

        var sut = CreateSut();

        await sut.Handle(
            new ResolveChronicConditionCommand(condition.Id),
            CancellationToken.None);

        Assert.False(condition.IsActive);
        _repoMock.Verify(r => r.Update(condition), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChronicConditionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ChronicCondition?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Handle(
            new ResolveChronicConditionCommand(ChronicConditionId.New()),
            CancellationToken.None));
    }
}
