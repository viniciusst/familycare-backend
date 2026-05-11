using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Authorization;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Mappings;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.MedicalHistory;
using MediatR;

namespace FamilyCare.Application.MedicalHistory.Queries.GetAppointmentsByMember;

public sealed record GetAppointmentsByMemberQuery(
    FamilyMemberId FamilyMemberId,
    int Page = 1,
    int PageSize = 20,
    AppointmentStatus? Status = null)
    : IQuery<PagedResult<AppointmentDto>>;

public sealed class GetAppointmentsByMemberQueryHandler(
    MedicalAccessGuard accessGuard,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<GetAppointmentsByMemberQuery, PagedResult<AppointmentDto>>
{
    private readonly MedicalAccessGuard _accessGuard = accessGuard;
    private readonly IAppointmentRepository _appointmentRepository = appointmentRepository;

    public async Task<PagedResult<AppointmentDto>> Handle(
        GetAppointmentsByMemberQuery request,
        CancellationToken cancellationToken)
    {
        await _accessGuard.EnsureCanReadAsync(
            request.FamilyMemberId, DataCategory.MedicalHistory, cancellationToken);

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _appointmentRepository.GetByMemberAsync(
            request.FamilyMemberId, pagination, request.Status, cancellationToken);

        return new PagedResult<AppointmentDto>(
            page.Items.Select(a => a.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}
