using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

public sealed class Exam : AggregateRoot<ExamId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public DateOnly ExamDate { get; private set; }
    public string ExamType { get; private set; }
    public string? Laboratory { get; private set; }
    public string? Results { get; private set; }
    public string? RequestedBy { get; private set; }

    private Exam() : base()
    {
        ExamType = null!;
    }

    private Exam(
        ExamId id,
        FamilyMemberId memberId,
        DateOnly examDate,
        string examType,
        string? laboratory,
        string? results,
        string? requestedBy) : base(id)
    {
        if (string.IsNullOrWhiteSpace(examType))
        {
            throw new InvalidEntityStateException("exam.type_required", "Exam type is required.");
        }

        FamilyMemberId = memberId;
        ExamDate = examDate;
        ExamType = examType.Trim();
        Laboratory = string.IsNullOrWhiteSpace(laboratory) ? null : laboratory.Trim();
        Results = string.IsNullOrWhiteSpace(results) ? null : results.Trim();
        RequestedBy = string.IsNullOrWhiteSpace(requestedBy) ? null : requestedBy.Trim();
    }

    public static Exam Register(
        FamilyMemberId memberId,
        DateOnly examDate,
        string examType,
        string? laboratory = null,
        string? results = null,
        string? requestedBy = null)
    {
        var exam = new Exam(ExamId.New(), memberId, examDate, examType, laboratory, results, requestedBy);
        exam.RaiseDomainEvent(new ExamRegisteredEvent(
            exam.Id, memberId, examDate, exam.ExamType, DateTime.UtcNow));
        return exam;
    }

    public void UpdateResults(string newResults)
    {
        if (string.IsNullOrWhiteSpace(newResults))
        {
            throw new InvalidEntityStateException("exam.results_empty", "Results cannot be empty.");
        }
        Results = newResults.Trim();
    }
}
