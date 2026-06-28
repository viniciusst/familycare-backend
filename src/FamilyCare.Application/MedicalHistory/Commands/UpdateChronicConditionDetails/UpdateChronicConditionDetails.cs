using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.UpdateChronicConditionDetails;

/// <summary>
/// Updates a chronic condition's editable details. Uses full-replacement
/// semantics: the caller sends the complete state, and nullable fields
/// passed as null are cleared.
///
/// The condition's lifecycle (Active/Resolved) is handled via the
/// dedicated Resolve endpoint, not through this update.
/// </summary>
public sealed record UpdateChronicConditionDetailsCommand(
    ChronicConditionId ChronicConditionId,
    string NewName,
    DateOnly NewDiagnosedAt,
    string? NewNotes)
    : ICommand;

public sealed class UpdateChronicConditionDetailsCommandValidator
    : AbstractValidator<UpdateChronicConditionDetailsCommand>
{
    public UpdateChronicConditionDetailsCommandValidator()
    {
        RuleFor(x => x.NewName)
            .NotEmpty()
            .MaximumLength(120);

        When(x => x.NewNotes is not null, () =>
        {
            RuleFor(x => x.NewNotes!)
                .MaximumLength(1000);
        });
    }
}

public sealed class UpdateChronicConditionDetailsCommandHandler(
    MedicalAccessGuard accessGuard,
    IChronicConditionRepository chronicConditionRepository)
    : IRequestHandler<UpdateChronicConditionDetailsCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IChronicConditionRepository _chronicConditionRepository = chronicConditionRepository;

    public async Task Handle(
        UpdateChronicConditionDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var condition = await _chronicConditionRepository.GetByIdAsync(
            request.ChronicConditionId, cancellationToken)
            ?? throw new NotFoundException(nameof(ChronicCondition), request.ChronicConditionId);

        await _accessGuard.EnsureCanWriteAsync(
            condition.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        condition.UpdateDetails(
            request.NewName,
            request.NewDiagnosedAt,
            request.NewNotes);

        _chronicConditionRepository.Update(condition);
    }
}
