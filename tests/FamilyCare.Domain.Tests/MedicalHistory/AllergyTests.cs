namespace FamilyCare.Domain.Tests.MedicalHistory;

public class AllergyTests
{
    private static Allergy Register(AllergySeverity severity = AllergySeverity.Mild)
        => Allergy.Register(
            memberId: FamilyMemberId.New(),
            substance: "Peanuts",
            severity: severity,
            reaction: "Hives",
            firstObservedAt: new DateOnly(2010, 5, 15));

    [Fact]
    public void Register_ShouldCreateAllergy()
    {
        var allergy = Register(AllergySeverity.Severe);

        Assert.Equal("Peanuts", allergy.Substance);
        Assert.Equal(AllergySeverity.Severe, allergy.Severity);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Register_WithEmptySubstance_ShouldThrow(string substance)
    {
        Assert.Throws<InvalidEntityStateException>(() => Allergy.Register(
            FamilyMemberId.New(),
            substance,
            AllergySeverity.Mild));
    }

    [Fact]
    public void ChangeSeverity_ShouldUpdateSeverity()
    {
        var allergy = Register(AllergySeverity.Mild);

        allergy.ChangeSeverity(AllergySeverity.Severe);

        Assert.Equal(AllergySeverity.Severe, allergy.Severity);
    }
}
