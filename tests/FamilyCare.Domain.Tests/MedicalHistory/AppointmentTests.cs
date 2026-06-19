using FamilyCare.Domain.MedicalHistory.Events;

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

    [Fact]
    public void UpdateDetails_WithAllFields_UpdatesAppointment()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old");

        appointment.UpdateDetails(
            doctorName: "Dr. New",
            specialty: "Cardiology",
            location: "New Clinic",
            notes: "Bring lab results");

        Assert.Equal("Dr. New", appointment.DoctorName);
        Assert.Equal("Cardiology", appointment.Specialty);
        Assert.Equal("New Clinic", appointment.Location);
        Assert.Equal("Bring lab results", appointment.Notes);
    }

    [Fact]
    public void UpdateDetails_WithOnlyDoctorName_KeepsOtherFields()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old",
            location: "Original Location",
            notes: "Original Notes");

        appointment.UpdateDetails(doctorName: "Dr. New", null, null, null);

        Assert.Equal("Dr. New", appointment.DoctorName);
        Assert.Equal("Dentistry", appointment.Specialty);
        Assert.Equal("Original Location", appointment.Location);
        Assert.Equal("Original Notes", appointment.Notes);
    }

    [Fact]
    public void UpdateDetails_WithEmptyLocation_SetsLocationToNull()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old",
            location: "Original Location");

        appointment.UpdateDetails(null, null, location: "   ", null);

        Assert.Null(appointment.Location);
    }

    [Fact]
    public void UpdateDetails_OnCompletedAppointment_Throws()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old");
        appointment.MarkCompleted();

        Assert.Throws<BusinessRuleViolationException>(
        () => appointment.UpdateDetails("Dr. New", null, null, null));
    }

    [Fact]
    public void UpdateDetails_OnCancelledAppointment_Throws()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old");
        appointment.Cancel();

        Assert.Throws<BusinessRuleViolationException>(
            () => appointment.UpdateDetails("Dr. New", null, null, null));
    }

    [Fact]
    public void UpdateDetails_WithAllNullFields_Throws()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old");

        Assert.Throws<BusinessRuleViolationException>(
            () => appointment.UpdateDetails(null, null, null, null));
    }

    [Fact]
    public void UpdateDetails_RaisesDomainEvent()
    {
        var appointment = Appointment.Schedule(
            FamilyMemberId.New(),
            DateTime.UtcNow.AddDays(7),
            "Dentistry",
            "Dr. Old");
        appointment.ClearDomainEvents();

        appointment.UpdateDetails("Dr. New", null, null, null);

        Assert.Single(appointment.DomainEvents);
        Assert.IsType<AppointmentDetailsUpdatedEvent>(appointment.DomainEvents.First());
    }
}
