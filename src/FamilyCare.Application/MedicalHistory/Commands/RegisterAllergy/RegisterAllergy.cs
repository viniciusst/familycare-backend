using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.RegisterAllergy;

public sealed record RegisterAllergyCommand(
    FamilyMemberId FamilyMemberId,
    string Substance,
    AllergySeverity Severity,
    string? Reaction = null,
    DateOnly? FirstObservedAt = null)
    : ICommand<AllergyId>;

public sealed class RegisterAllergyCommandValidator : AbstractValidator<RegisterAllergyCommand>
{
    public RegisterAllergyCommandValidator()
    {
        RuleFor(x => x.Substance).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Reaction).MaximumLength(2000);
    }
}

public sealed class RegisterAllergyCommandHandler(
    MedicalAccessGuard accessGuard,
    IAllergyRepository allergyRepository)
    : IRequestHandler<RegisterAllergyCommand, AllergyId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAllergyRepository _allergyRepository = allergyRepository;

    public async Task<AllergyId> Handle(RegisterAllergyCommand request, CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanWriteAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var allergy = Allergy.Register(
            request.FamilyMemberId,
            request.Substance,
            request.Severity,
            request.Reaction,
            request.FirstObservedAt);

        await _allergyRepository.AddAsync(allergy, cancellationToken);
        return allergy.Id;
    }
}
