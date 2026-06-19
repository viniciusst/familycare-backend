using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetFamilyInvitations;

/// <summary>
/// Lists invitations of a given family. Only Owner/Admin can call this.
/// </summary>
public sealed record GetFamilyInvitationsQuery(
    FamilyId FamilyId,
    int Page = 1,
    int PageSize = 20,
    InvitationStatus? Status = null)
    : IQuery<PagedResult<InvitationDto>>;

public sealed class GetFamilyInvitationsQueryHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetFamilyInvitationsQuery, PagedResult<InvitationDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<PagedResult<InvitationDto>> Handle(
        GetFamilyInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        // Authorization: only Owner/Admin can see the invitation list.
        var caller = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        if (caller.Role != Role.Owner && caller.Role != Role.Admin)
        {
            throw new ForbiddenException(
                "Only the family owner or admins can list invitations.");
        }

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _familyRepository.GetInvitationsByFamilyAsync(
            request.FamilyId, request.Status, pagination, cancellationToken);

        return new PagedResult<InvitationDto>(
            page.Items.Select(i => i.ToDto()).ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}