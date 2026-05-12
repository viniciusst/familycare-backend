namespace FamilyCare.Domain.Tests.MedicalHistory;

public class VaccineTests
{
    private static Vaccine Register(DateOnly? nextDoseDue = null)
        => Vaccine.Register(
            memberId: FamilyMemberId.New(),
            name: "COVID-19 Pfizer",
            appliedAt: DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            manufacturer: "Pfizer",
            batchNumber: "ABC123",
            doseNumber: 2,
            nextDoseDue: nextDoseDue,
            notes: null);

    [Fact]
    public void Register_ShouldCreateVaccine()
    {
        var vaccine = Register();

        Assert.Equal("COVID-19 Pfizer", vaccine.Name);
        Assert.Equal(2, vaccine.DoseNumber);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Register_WithEmptyName_ShouldThrow(string name)
    {
        Assert.Throws<InvalidEntityStateException>(() => Vaccine.Register(
            FamilyMemberId.New(),
            name,
            DateOnly.FromDateTime(DateTime.UtcNow)));
    }
}
