using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UpdateAllergyDetails;

/// <summary>
/// Updates an allergy's editable details. Uses a full-replacement
/// semantics: the caller sends the complete state, and nullable fields
/// passed as null are cleared. Severity is updated via its own dedicated
/// endpoint (ChangeAllergySeverity).
/// </summary>
public sealed record UpdateAllergyDetailsCommand(
    AllergyId AllergyId,
    string NewSubstance,
    string? NewReaction,
    DateOnly? NewFirstObservedAt)
    : ICommand;

public sealed class UpdateAllergyDetailsCommandValidator
    : AbstractValidator<UpdateAllergyDetailsCommand>
{
    public UpdateAllergyDetailsCommandValidator()
    {
        RuleFor(x => x.NewSubstance)
            .NotEmpty()
            .MaximumLength(120);

        When(x => x.NewReaction is not null, () =>
        {
            RuleFor(x => x.NewReaction!)
                .MaximumLength(500);
        });
    }
}

public sealed class UpdateAllergyDetailsCommandHandler(
    MedicalAccessGuard accessGuard,
    IAllergyRepository allergyRepository)
    : IRequestHandler<UpdateAllergyDetailsCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAllergyRepository _allergyRepository = allergyRepository;

    public async Task Handle(
        UpdateAllergyDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var allergy = await _allergyRepository.GetByIdAsync(
            request.AllergyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Allergy), request.AllergyId);

        await _accessGuard.EnsureCanWriteAsync(
            allergy.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        allergy.UpdateDetails(
            request.NewSubstance,
            request.NewReaction,
            request.NewFirstObservedAt);

        _allergyRepository.Update(allergy);
    }
}
