using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.RegisterChronicCondition;

public sealed record RegisterChronicConditionCommand(
    FamilyMemberId FamilyMemberId,
    string Name,
    DateOnly DiagnosedAt,
    string? Notes = null)
    : ICommand<ChronicConditionId>;

public sealed class RegisterChronicConditionCommandValidator : AbstractValidator<RegisterChronicConditionCommand>
{
    public RegisterChronicConditionCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Notes).MaximumLength(4000);

        RuleFor(x => x.DiagnosedAt)
            .LessThanOrEqualTo(_ => dateTimeProvider.TodayUtc)
            .WithMessage("Diagnosis date cannot be in the future.");
    }
}

public sealed class RegisterChronicConditionCommandHandler(
    MedicalAccessGuard accessGuard,
    IChronicConditionRepository conditionRepository)
    : IRequestHandler<RegisterChronicConditionCommand, ChronicConditionId>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IChronicConditionRepository _conditionRepository = conditionRepository;

    public async Task<ChronicConditionId> Handle(
        RegisterChronicConditionCommand request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanWriteAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var condition = ChronicCondition.Register(
            request.FamilyMemberId,
            request.Name,
            request.DiagnosedAt,
            request.Notes);

        await _conditionRepository.AddAsync(condition, cancellationToken);
        return condition.Id;
    }
}
