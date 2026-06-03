namespace FamilyCare.Domain.Tests.MedicalHistory;

public class ExamTests
{
    private static Exam Register(string? results = null)
        => Exam.Register(
            memberId: FamilyMemberId.New(),
            examDate: DateOnly.FromDateTime(DateTime.UtcNow),
            examType: "Blood Test",
            laboratory: "LabCorp",
            results: results,
            requestedBy: "Dr. Smith");

    [Fact]
    public void Register_ShouldCreateExam()
    {
        var exam = Register();

        Assert.Equal("Blood Test", exam.ExamType);
        Assert.Equal("LabCorp", exam.Laboratory);
        Assert.Equal("Dr. Smith", exam.RequestedBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Register_WithEmptyType_ShouldThrow(string type)
    {
        Assert.Throws<InvalidEntityStateException>(() => Exam.Register(
            FamilyMemberId.New(),
            DateOnly.FromDateTime(DateTime.UtcNow),
            type));
    }
}
