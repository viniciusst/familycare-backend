using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetAllergiesByMember;

public sealed record GetAllergiesByMemberQuery(FamilyMemberId FamilyMemberId)
    : IQuery<IReadOnlyList<AllergyDto>>;

public sealed class GetAllergiesByMemberQueryHandler(
    MedicalAccessGuard accessGuard,
    IAllergyRepository allergyRepository)
    : IRequestHandler<GetAllergiesByMemberQuery, IReadOnlyList<AllergyDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAllergyRepository _allergyRepository = allergyRepository;

    public async Task<IReadOnlyList<AllergyDto>> Handle(
        GetAllergiesByMemberQuery request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanReadAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var items = await _allergyRepository.GetByMemberAsync(
            request.FamilyMemberId, cancellationToken);

        return items.Select(a => a.ToDto()).ToList();
    }
}
