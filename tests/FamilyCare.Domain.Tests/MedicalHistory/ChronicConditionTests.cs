namespace FamilyCare.Domain.Tests.MedicalHistory;

public class ChronicConditionTests
{
    private static ChronicCondition Register()
        => ChronicCondition.Register(
            memberId: FamilyMemberId.New(),
            name: "Hypertension",
            diagnosedAt: new DateOnly(2020, 1, 1),
            notes: "Mild");

    [Fact]
    public void Register_ShouldCreateInActiveState()
    {
        var condition = Register();

        Assert.True(condition.IsActive);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Register_WithEmptyName_ShouldThrow(string name)
    {
        Assert.Throws<InvalidEntityStateException>(() => ChronicCondition.Register(
            FamilyMemberId.New(),
            name,
            new DateOnly(2020, 1, 1)));
    }

    [Fact]
    public void Resolve_ShouldMarkInactive()
    {
        var condition = Register();

        condition.Resolve();

        Assert.False(condition.IsActive);
    }

    [Fact]
    public void Reactivate_ShouldMarkActiveAgain()
    {
        var condition = Register();
        condition.Resolve();

        condition.Reactivate();

        Assert.True(condition.IsActive);
    }
}
