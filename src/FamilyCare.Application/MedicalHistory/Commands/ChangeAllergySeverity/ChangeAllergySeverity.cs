using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.ChangeAllergySeverity;

public sealed record ChangeAllergySeverityCommand(
    AllergyId AllergyId,
    AllergySeverity NewSeverity)
    : ICommand;

public sealed class ChangeAllergySeverityCommandValidator : AbstractValidator<ChangeAllergySeverityCommand>
{
    public ChangeAllergySeverityCommandValidator()
    {
        RuleFor(x => x.NewSeverity).IsInEnum();
    }
}

public sealed class ChangeAllergySeverityCommandHandler(
    MedicalAccessGuard accessGuard,
    IAllergyRepository allergyRepository)
    : IRequestHandler<ChangeAllergySeverityCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAllergyRepository _allergyRepository = allergyRepository;

    public async Task Handle(ChangeAllergySeverityCommand request, CancellationToken cancellationToken)
    {
        var allergy = await _allergyRepository.GetByIdAsync(request.AllergyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Allergy), request.AllergyId);

        await _accessGuard.EnsureCanWriteAsync(
            allergy.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        allergy.ChangeSeverity(request.NewSeverity);
        _allergyRepository.Update(allergy);
    }
}
