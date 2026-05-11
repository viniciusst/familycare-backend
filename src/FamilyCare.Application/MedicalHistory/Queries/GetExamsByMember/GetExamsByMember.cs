using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetExamsByMember;

public sealed record GetExamsByMemberQuery(
    FamilyMemberId FamilyMemberId,
    int Page = 1,
    int PageSize = 20,
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IQuery<PagedResult<ExamDto>>;

public sealed class GetExamsByMemberQueryHandler(
    MedicalAccessGuard accessGuard,
    IExamRepository examRepository)
    : IRequestHandler<GetExamsByMemberQuery, PagedResult<ExamDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IExamRepository _examRepository = examRepository;

    public async Task<PagedResult<ExamDto>> Handle(
        GetExamsByMemberQuery request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanReadAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _examRepository.GetByMemberAsync(
            request.FamilyMemberId, pagination, request.FromDate, request.ToDate, cancellationToken);

        return new PagedResult<ExamDto>(
            page.Items.Select(e => e.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
