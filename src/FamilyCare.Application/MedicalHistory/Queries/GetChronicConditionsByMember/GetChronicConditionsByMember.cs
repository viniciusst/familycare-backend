using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetChronicConditionsByMember;

public sealed record GetChronicConditionsByMemberQuery(
    FamilyMemberId FamilyMemberId,
    bool? ActiveOnly = null)
    : IQuery<IReadOnlyList<ChronicConditionDto>>;

public sealed class GetChronicConditionsByMemberQueryHandler(
    MedicalAccessGuard accessGuard,
    IChronicConditionRepository conditionRepository)
    : IRequestHandler<GetChronicConditionsByMemberQuery, IReadOnlyList<ChronicConditionDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IChronicConditionRepository _conditionRepository = conditionRepository;

    public async Task<IReadOnlyList<ChronicConditionDto>> Handle(
        GetChronicConditionsByMemberQuery request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanReadAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var items = await _conditionRepository.GetByMemberAsync(
            request.FamilyMemberId, request.ActiveOnly, cancellationToken);

        return items.Select(c => c.ToDto()).ToList();
    }
}
