using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetInvitationById;

/// <summary>
/// Returns a single invitation by id. Visible to the invitee (by email)
/// or to Owner/Admin of the inviting family.
/// </summary>
public sealed record GetInvitationByIdQuery(InvitationId InvitationId)
    : IQuery<InvitationDetailsDto>;

public sealed class GetInvitationByIdQueryHandler(
    ICurrentUserService currentUserService,
    IUserRepository userRepository,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetInvitationByIdQuery, InvitationDetailsDto>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<InvitationDetailsDto> Handle(
        GetInvitationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var result = await _familyRepository.FindInvitationByIdAsync(
            request.InvitationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invitation), request.InvitationId);

        var (family, invitation) = result;

        // Authorization: invitee OR Owner/Admin of the family.
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user not found.");

        bool isInvitee = string.Equals(
            invitation.Email.Value,
            user.Email.Value,
            StringComparison.OrdinalIgnoreCase);

        var member = family.Members.FirstOrDefault(m => m.UserId == userId);
        bool isOwnerOrAdmin = member is not null
            && (member.Role == Role.Owner || member.Role == Role.Admin);

        if (!isInvitee && !isOwnerOrAdmin)
        {
            throw new ForbiddenException(
                "You don't have access to this invitation.");
        }

        return invitation.ToDetailsDto(family.Id, family.Name);
    }
}