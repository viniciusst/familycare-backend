using FamilyCare.Application.MedicalHistory.Commands.RegisterExam;
using FamilyCare.Application.MedicalHistory.Commands.RegisterVaccine;
using FamilyCare.Application.MedicalHistory.Commands.UpdateExamResults;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.MedicalHistory.Commands;

public class RegisterExamCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IExamRepository> _repoMock = new();

    private RegisterExamCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldRegisterAndPersist()
    {
        _guard.WithAllAccessGranted();
        var sut = CreateSut();

        var command = new RegisterExamCommand(
            FamilyMemberId.New(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Blood Test",
            Laboratory: "LabCorp");

        var examId = await sut.Handle(command, CancellationToken.None);

        Assert.NotEqual(default, examId);
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<Exam>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class RegisterExamCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly RegisterExamCommandValidator _validator;

    public RegisterExamCommandValidatorTests()
    {
        _clockMock.Setup(c => c.TodayUtc).Returns(new DateOnly(2026, 5, 30));
        _validator = new RegisterExamCommandValidator(_clockMock.Object);
    }

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new RegisterExamCommand(
            FamilyMemberId.New(),
            new DateOnly(2026, 5, 1),
            "Blood Test"));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithFutureExamDate_ShouldFail()
    {
        var result = _validator.Validate(new RegisterExamCommand(
            FamilyMemberId.New(),
            new DateOnly(2027, 1, 1),
            "Blood Test"));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyExamType_ShouldFail()
    {
        var result = _validator.Validate(new RegisterExamCommand(
            FamilyMemberId.New(),
            new DateOnly(2026, 5, 1),
            ""));

        Assert.False(result.IsValid);
    }
}

public class UpdateExamResultsCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IExamRepository> _repoMock = new();

    private UpdateExamResultsCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldUpdateResults()
    {
        var exam = Exam.Register(
            FamilyMemberId.New(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Blood Test");

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(exam.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(exam);

        var sut = CreateSut();

        await sut.Handle(
            new UpdateExamResultsCommand(exam.Id, "Updated values"),
            CancellationToken.None);

        Assert.Equal("Updated values", exam.Results);
        _repoMock.Verify(r => r.Update(exam), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ExamId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Handle(
            new UpdateExamResultsCommand(ExamId.New(), "Anything"),
            CancellationToken.None));
    }
}

public class RegisterVaccineCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IVaccineRepository> _repoMock = new();

    private RegisterVaccineCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldRegisterAndPersist()
    {
        _guard.WithAllAccessGranted();
        var sut = CreateSut();

        var command = new RegisterVaccineCommand(
            FamilyMemberId.New(),
            "COVID-19",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        var vaccineId = await sut.Handle(command, CancellationToken.None);

        Assert.NotEqual(default, vaccineId);
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<Vaccine>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

public class RegisterVaccineCommandValidatorTests
{
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly RegisterVaccineCommandValidator _validator;

    public RegisterVaccineCommandValidatorTests()
    {
        _clockMock.Setup(c => c.TodayUtc).Returns(new DateOnly(2026, 5, 30));
        _validator = new RegisterVaccineCommandValidator(_clockMock.Object);
    }

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new RegisterVaccineCommand(
            FamilyMemberId.New(),
            "COVID-19",
            new DateOnly(2026, 1, 1)));

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNextDoseBeforeApplied_ShouldFail()
    {
        var result = _validator.Validate(new RegisterVaccineCommand(
            FamilyMemberId.New(),
            "COVID-19",
            AppliedAt: new DateOnly(2026, 1, 1),
            NextDoseDue: new DateOnly(2025, 1, 1)));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WithDoseNumberZero_ShouldFail()
    {
        var result = _validator.Validate(new RegisterVaccineCommand(
            FamilyMemberId.New(),
            "COVID-19",
            new DateOnly(2026, 1, 1),
            DoseNumber: 0));

        Assert.False(result.IsValid);
    }
}
