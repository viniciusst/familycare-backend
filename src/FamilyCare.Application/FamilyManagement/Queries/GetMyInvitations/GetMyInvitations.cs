using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetMyInvitations;

/// <summary>
/// Returns invitations addressed to the current user (by email).
/// Used by the recipient's inbox UI.
/// </summary>
public sealed record GetMyInvitationsQuery(
    int Page = 1,
    int PageSize = 20,
    InvitationStatus? Status = null)
    : IQuery<PagedResult<InvitationDetailsDto>>;

public sealed class GetMyInvitationsQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetMyInvitationsQuery, PagedResult<InvitationDetailsDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<PagedResult<InvitationDetailsDto>> Handle(
        GetMyInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        // We need the user's email to match invitations. Invitations are
        // addressed to an email, not a user id, since the invitee may not
        // have an account yet at invite time.
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user not found.");

        var pagination = new PagedRequest(request.Page, request.PageSize);
        var page = await _familyRepository.GetInvitationsByEmailAsync(
            user.Email.Value, 
            request.Status,
            pagination,
            cancellationToken);

        return new PagedResult<InvitationDetailsDto>(
            page.Items
                .Select(pair => pair.Invitation.ToDetailsDto(pair.Family.Id, pair.Family.Name))
                .ToList(),
            page.Page,
            page.PageSize,
            page.TotalCount);
    }
}