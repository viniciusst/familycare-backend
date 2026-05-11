using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Commands.ResolveChronicCondition;

public sealed record ResolveChronicConditionCommand(ChronicConditionId Id) : ICommand;

public sealed class ResolveChronicConditionCommandHandler(
    MedicalAccessGuard accessGuard,
    IChronicConditionRepository conditionRepository)
    : IRequestHandler<ResolveChronicConditionCommand>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IChronicConditionRepository _conditionRepository = conditionRepository;

    public async Task Handle(ResolveChronicConditionCommand request, CancellationToken cancellationToken)
    {
        var condition = await _conditionRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(ChronicCondition), request.Id);

        await _accessGuard.EnsureCanWriteAsync(
            condition.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        condition.Resolve();
        _conditionRepository.Update(condition);
    }
}
