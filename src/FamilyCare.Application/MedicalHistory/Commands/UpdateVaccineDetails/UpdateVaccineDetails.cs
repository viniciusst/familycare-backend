using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UpdateVaccineDetails;

/// <summary>
/// Updates a vaccine record's editable details. Uses full-replacement
/// semantics: the caller sends the complete state, and nullable fields
/// passed as null are cleared.
///
/// This is for correcting typos and miscategorizations — the underlying
/// fact that a vaccine was administered remains. A new dose is registered
/// as a separate Vaccine row with an incremented doseNumber, not through
/// this update.
/// </summary>
public sealed record UpdateVaccineDetailsCommand(
    VaccineId VaccineId,
    string NewName,
    DateOnly NewAppliedAt,
    string? NewManufacturer,
    string? NewBatchNumber,
    int? NewDoseNumber,
    DateOnly? NewNextDoseDue,
    string? NewNotes)
    : ICommand;

public sealed class UpdateVaccineDetailsCommandValidator
    : AbstractValidator<UpdateVaccineDetailsCommand>
{
    public UpdateVaccineDetailsCommandValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(120);

        When(x => x.NewManufacturer is not null, () =>
        {
            RuleFor(x => x.NewManufacturer!)
                .MaximumLength(120);
        });

        When(x => x.NewBatchNumber is not null, () =>
        {
            RuleFor(x => x.NewBatchNumber!)
                .MaximumLength(60);
        });

        When(x => x.NewDoseNumber is not null, () =>
        {
            RuleFor(x => x.NewDoseNumber!.Value)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Dose number must be at least 1.");
        });

        When(x => x.NewNextDoseDue is not null, () =>
        {
            RuleFor(x => x)
                .Must(x => x.NewNextDoseDue!.Value >= x.NewAppliedAt)
                .WithMessage("Next dose date cannot be before the application date.");
        });

        When(x => x.NewNotes is not null, () =>
        {
            RuleFor(x => x.NewNotes!)
                .MaximumLength(500);
        });
    }
}

public sealed class UpdateVaccineDetailsCommandHandler(
    MedicalAccessGuard accessGuard,
    IVaccineRepository vaccineRepository)
    : IRequestHandler<UpdateVaccineDetailsCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IVaccineRepository _vaccineRepository = vaccineRepository;

    public async Task Handle(
        UpdateVaccineDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var vaccine = await _vaccineRepository.GetByIdAsync(
            request.VaccineId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vaccine), request.VaccineId);

        await _accessGuard.EnsureCanWriteAsync(
            vaccine.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        vaccine.UpdateDetails(
            request.NewName,
            request.NewAppliedAt,
            request.NewManufacturer,
            request.NewBatchNumber,
            request.NewDoseNumber,
            request.NewNextDoseDue,
            request.NewNotes);

        _vaccineRepository.Update(vaccine);
    }
}
