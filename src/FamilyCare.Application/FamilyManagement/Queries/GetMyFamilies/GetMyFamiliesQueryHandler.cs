using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetMyFamilies;

public sealed class GetMyFamiliesQueryHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetMyFamiliesQuery, PagedResult<FamilySummaryDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<PagedResult<FamilySummaryDto>> Handle(
        GetMyFamiliesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _familyRepository.GetByUserIdAsync(userId, pagination, cancellationToken);

        return new PagedResult<FamilySummaryDto>(
            page.Items.Select(f => f.ToSummaryDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
