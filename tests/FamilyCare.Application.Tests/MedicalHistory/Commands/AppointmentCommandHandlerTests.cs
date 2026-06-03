using FamilyCare.Application.MedicalHistory.Commands.CancelAppointment;
using FamilyCare.Application.MedicalHistory.Commands.CompleteAppointment;
using FamilyCare.Application.MedicalHistory.Commands.RescheduleAppointment;
using FamilyCare.Application.MedicalHistory.Commands.ScheduleAppointment;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Application.Tests.Common;

namespace FamilyCare.Application.Tests.MedicalHistory.Commands;

public class ScheduleAppointmentCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAppointmentRepository> _repoMock = new();

    private ScheduleAppointmentCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldScheduleAndPersist()
    {
        // Arrange
        _guard.WithAllAccessGranted();
        var sut = CreateSut();

        var command = new ScheduleAppointmentCommand(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Cardiology",
            "Dr. House",
            "Clinic A",
            "Annual checkup");

        // Act
        var appointmentId = await sut.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(default, appointmentId);
        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenWriteForbidden_ShouldThrowForbidden()
    {
        // Arrange — privacy evaluator denies write.
        _guard.WithAllAccessGranted();
        _guard.PrivacyEvaluator
            .Setup(p => p.CanWriteAsync(
                It.IsAny<FamilyId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<FamilyMemberId>(),
                It.IsAny<DataCategory>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var sut = CreateSut();
        var command = new ScheduleAppointmentCommand(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Cardiology",
            "Dr. House");

        // Act + Assert
        await Assert.ThrowsAsync<ForbiddenException>(
            () => sut.Handle(command, CancellationToken.None));

        _repoMock.Verify(
            r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMemberNotFound_ShouldThrowNotFound()
    {
        // Arrange — resolver can't find the family.
        _guard.CurrentUser.Setup(c => c.RequireUserId()).Returns(UserId.New());
        _guard.MembershipResolver
            .Setup(r => r.GetFamilyIdForMemberAsync(
                It.IsAny<FamilyMemberId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FamilyId?)null);

        var sut = CreateSut();
        var command = new ScheduleAppointmentCommand(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Cardiology",
            "Dr. House");

        // Act + Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => sut.Handle(command, CancellationToken.None));
    }
}

public class ScheduleAppointmentCommandValidatorTests
{
    private readonly ScheduleAppointmentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidInput_ShouldPass()
    {
        var result = _validator.Validate(new ScheduleAppointmentCommand(
            FamilyMemberId.New(), DateTime.UtcNow.AddDays(7),
            "Cardiology", "Dr. House"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    public void Validate_WithEmptySpecialty_ShouldFail(string specialty)
    {
        var result = _validator.Validate(new ScheduleAppointmentCommand(
            FamilyMemberId.New(), DateTime.UtcNow.AddDays(7),
            specialty, "Dr. House"));

        Assert.False(result.IsValid);
    }
}

public class CompleteAppointmentCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAppointmentRepository> _repoMock = new();

    private CompleteAppointmentCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    private static Appointment AnyAppointment()
        => Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(1),
            "Cardiology",
            "Dr. House");

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldMarkCompleted()
    {
        // Arrange
        var appointment = AnyAppointment();
        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var sut = CreateSut();

        // Act
        await sut.Handle(
            new CompleteAppointmentCommand(appointment.Id, "All good"),
            CancellationToken.None);

        // Assert
        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
        _repoMock.Verify(r => r.Update(appointment), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ShouldThrowNotFound()
    {
        _repoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<AppointmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var sut = CreateSut();

        await Assert.ThrowsAsync<NotFoundException>(() => sut.Handle(
            new CompleteAppointmentCommand(AppointmentId.New(), null),
            CancellationToken.None));
    }
}

public class CancelAppointmentCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAppointmentRepository> _repoMock = new();

    private CancelAppointmentCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldCancel()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(), DateTime.UtcNow.AddDays(1), "Cardiology", "Dr. House");

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var sut = CreateSut();

        await sut.Handle(new CancelAppointmentCommand(appointment.Id), CancellationToken.None);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        _repoMock.Verify(r => r.Update(appointment), Times.Once);
    }
}

public class RescheduleAppointmentCommandHandlerTests
{
    private readonly MedicalAccessGuardBuilder _guard = new();
    private readonly Mock<IAppointmentRepository> _repoMock = new();

    private RescheduleAppointmentCommandHandler CreateSut()
        => new(_guard.Build(), _repoMock.Object);

    [Fact]
    public async Task Handle_WhenAuthorized_ShouldUpdateScheduledAt()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(), DateTime.UtcNow.AddDays(1), "Cardiology", "Dr. House");
        var newDate = DateTime.UtcNow.AddDays(14);

        _guard.WithAllAccessGranted();
        _repoMock
            .Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var sut = CreateSut();

        await sut.Handle(
            new RescheduleAppointmentCommand(appointment.Id, newDate),
            CancellationToken.None);

        Assert.InRange(
            appointment.ScheduledAt,
            newDate.AddSeconds(-1),
            newDate.AddSeconds(1));
    }
}
