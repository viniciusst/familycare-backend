using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory.Events;

namespace FamilyCare.Domain.MedicalHistory;

public sealed class Vaccine : AggregateRoot<VaccineId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public string Name { get; private set; }
    public DateOnly AppliedAt { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? BatchNumber { get; private set; }
    public int? DoseNumber { get; private set; }
    public DateOnly? NextDoseDue { get; private set; }
    public string? Notes { get; private set; }

    private Vaccine() : base()
    {
        Name = null!;
    }

    private Vaccine(
        VaccineId id,
        FamilyMemberId memberId,
        string name,
        DateOnly appliedAt,
        string? manufacturer,
        string? batchNumber,
        int? doseNumber,
        DateOnly? nextDoseDue,
        string? notes) : base(id)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidEntityStateException("vaccine.name_required", "Vaccine name is required.");
        }

        if (doseNumber is not null && doseNumber.Value < 1)
        {
            throw new InvalidEntityStateException("vaccine.invalid_dose", "Dose number must be >= 1.");
        }

        if (nextDoseDue is not null && nextDoseDue.Value < appliedAt)
        {
            throw new InvalidEntityStateException(
                "vaccine.invalid_next_dose",
                "Next dose date cannot be before the application date.");
        }

        FamilyMemberId = memberId;
        Name = name.Trim();
        AppliedAt = appliedAt;
        Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer.Trim();
        BatchNumber = string.IsNullOrWhiteSpace(batchNumber) ? null : batchNumber.Trim();
        DoseNumber = doseNumber;
        NextDoseDue = nextDoseDue;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }

    public static Vaccine Register(
        FamilyMemberId memberId,
        string name,
        DateOnly appliedAt,
        string? manufacturer = null,
        string? batchNumber = null,
        int? doseNumber = null,
        DateOnly? nextDoseDue = null,
        string? notes = null)
    {
        var vaccine = new Vaccine(
            VaccineId.New(), memberId, name, appliedAt,
            manufacturer, batchNumber, doseNumber, nextDoseDue, notes);

        vaccine.RaiseDomainEvent(new VaccineRegisteredEvent(
            vaccine.Id, memberId, vaccine.Name, appliedAt, DateTime.UtcNow));

        return vaccine;
    }
}
