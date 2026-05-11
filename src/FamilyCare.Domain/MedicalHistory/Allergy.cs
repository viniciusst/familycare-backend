using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

public sealed class Allergy : AggregateRoot<AllergyId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public string Substance { get; private set; }
    public AllergySeverity Severity { get; private set; }
    public string? Reaction { get; private set; }
    public DateOnly? FirstObservedAt { get; private set; }

    private Allergy() : base()
    {
        Substance = null!;
    }

    private Allergy(
        AllergyId id,
        FamilyMemberId memberId,
        string substance,
        AllergySeverity severity,
        string? reaction,
        DateOnly? firstObservedAt) : base(id)
    {
        if (string.IsNullOrWhiteSpace(substance))
        {
            throw new InvalidEntityStateException("allergy.substance_required", "Substance is required.");
        }

        FamilyMemberId = memberId;
        Substance = substance.Trim();
        Severity = severity;
        Reaction = string.IsNullOrWhiteSpace(reaction) ? null : reaction.Trim();
        FirstObservedAt = firstObservedAt;
    }

    public static Allergy Register(
        FamilyMemberId memberId,
        string substance,
        AllergySeverity severity,
        string? reaction = null,
        DateOnly? firstObservedAt = null)
    {
        var allergy = new Allergy(
            AllergyId.New(), memberId, substance, severity, reaction, firstObservedAt);

        allergy.RaiseDomainEvent(new AllergyRegisteredEvent(
            allergy.Id, memberId, allergy.Substance, severity, DateTime.UtcNow));

        return allergy;
    }

    public void ChangeSeverity(AllergySeverity newSeverity) => Severity = newSeverity;
}
