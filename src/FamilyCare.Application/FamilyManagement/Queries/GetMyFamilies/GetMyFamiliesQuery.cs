using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Dtos;

namespace FamilyCare.Application.FamilyManagement.Queries.GetMyFamilies;

public sealed record GetMyFamiliesQuery(int Page = 1, int PageSize = 20)
    : IQuery<PagedResult<FamilySummaryDto>>;
