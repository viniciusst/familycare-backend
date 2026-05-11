using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetVaccinesByMember;

public sealed record GetVaccinesByMemberQuery(
    FamilyMemberId FamilyMemberId,
    int Page = 1,
    int PageSize = 20)
    : IQuery<PagedResult<VaccineDto>>;

public sealed class GetVaccinesByMemberQueryHandler(
    MedicalAccessGuard accessGuard,
    IVaccineRepository vaccineRepository)
    : IRequestHandler<GetVaccinesByMemberQuery, PagedResult<VaccineDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IVaccineRepository _vaccineRepository = vaccineRepository;

    public async Task<PagedResult<VaccineDto>> Handle(
        GetVaccinesByMemberQuery request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanReadAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _vaccineRepository.GetByMemberAsync(
            request.FamilyMemberId, pagination, cancellationToken);

        return new PagedResult<VaccineDto>(
            page.Items.Select(v => v.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
