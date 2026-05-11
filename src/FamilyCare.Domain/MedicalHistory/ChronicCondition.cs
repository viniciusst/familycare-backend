using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

public sealed class ChronicCondition : AggregateRoot<ChronicConditionId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public string Name { get; private set; }
    public DateOnly DiagnosedAt { get; private set; }
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; }

    private ChronicCondition() : base()
    {
        Name = null!;
    }

    private ChronicCondition(
        ChronicConditionId id,
        FamilyMemberId memberId,
        string name,
        DateOnly diagnosedAt,
        string? notes) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidEntityStateException("chronic_condition.name_required", "Condition name is required.");
        }

        FamilyMemberId = memberId;
        Name = name.Trim();
        DiagnosedAt = diagnosedAt;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        IsActive = true;
    }

    public static ChronicCondition Register(
        FamilyMemberId memberId,
        string name,
        DateOnly diagnosedAt,
        string? notes = null)
    {
        var condition = new ChronicCondition(
            ChronicConditionId.New(), memberId, name, diagnosedAt, notes);

        condition.RaiseDomainEvent(new ChronicConditionRegisteredEvent(
            condition.Id, memberId, condition.Name, DateTime.UtcNow));

        return condition;
    }

    public void Resolve() => IsActive = false;

    public void Reactivate() => IsActive = true;

    public void UpdateNotes(string? newNotes)
        => Notes = string.IsNullOrWhiteSpace(newNotes) ? null : newNotes.Trim();
}
