using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository,
    IUserRepository userRepository)
    : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResponse>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task<AcceptInvitationResponse> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var lookup = await _familyRepository.FindPendingInvitationByIdAsync(
            request.InvitationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invitation), request.InvitationId);

        var (family, invitation) = lookup;

        // Verify the authenticated user matches the invited email.
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!user.Email.Equals(invitation.Email))
        {
            throw new ForbiddenException(
                "This invitation was sent to a different email address.");
        }

        // Reject if user is already a member of this family.
        if (family.Members.Any(m => m.UserId == userId))
        {
            throw new ConflictException(
                "family.member.already_in_family",
                "You are already a member of this family.");
        }

        var member = family.AcceptInvitation(
            request.InvitationId,
            userId,
            request.DisplayName,
            request.BirthDate);

        _familyRepository.Update(family);

        return new AcceptInvitationResponse(family.Id, member.Id);
    }
}
