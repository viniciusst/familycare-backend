namespace FamilyCare.Domain.Tests.MedicalHistory;

public class AppointmentTests
{
    private static Appointment Schedule(DateTime? at = null)
        => Appointment.Schedule(
            memberId: FamilyMemberId.New(),
            scheduledAt: at ?? DateTime.UtcNow.AddDays(7),
            specialty: "Cardiology",
            doctorName: "Dr. House",
            location: "Clinic A",
            notes: null);

    [Fact]
    public void Schedule_ShouldCreateInScheduledState()
    {
        var appointment = Schedule();

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal("Cardiology", appointment.Specialty);
        Assert.Equal("Dr. House", appointment.DoctorName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Schedule_WithEmptySpecialty_ShouldThrow(string specialty)
    {
        Assert.Throws<InvalidEntityStateException>(() => Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(1),
            specialty,
            "Doc"));
    }

    [Fact]
    public void MarkCompleted_FromScheduled_ShouldTransitionToCompleted()
    {
        var appointment = Schedule();

        appointment.MarkCompleted("All good");

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public void MarkCompleted_FromCompleted_ShouldThrow()
    {
        var appointment = Schedule();
        appointment.MarkCompleted(null);

        Assert.Throws<BusinessRuleViolationException>(() => appointment.MarkCompleted(null));
    }

    [Fact]
    public void Cancel_FromScheduled_ShouldTransitionToCancelled()
    {
        var appointment = Schedule();

        appointment.Cancel();

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
    }

    [Fact]
    public void Cancel_FromCompleted_ShouldThrow()
    {
        var appointment = Schedule();
        appointment.MarkCompleted(null);

        Assert.Throws<BusinessRuleViolationException>(() => appointment.Cancel());
    }

    [Fact]
    public void Reschedule_ToFuture_ShouldUpdateScheduledAt()
    {
        var appointment = Schedule();
        var newDate = DateTime.UtcNow.AddDays(14);

        appointment.Reschedule(newDate);

        Assert.InRange(
            appointment.ScheduledAt,
            newDate.AddSeconds(-1),
            newDate.AddSeconds(1));
    }

    [Fact]
    public void Reschedule_AfterCompletion_ShouldThrow()
    {
        var appointment = Schedule();
        appointment.MarkCompleted(null);

        Assert.Throws<BusinessRuleViolationException>(
            () => appointment.Reschedule(DateTime.UtcNow.AddDays(14)));
    }
}
