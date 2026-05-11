using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.DeclineInvitation;

public sealed class DeclineInvitationCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository,
    IUserRepository userRepository)
    : IRequestHandler<DeclineInvitationCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task Handle(DeclineInvitationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var lookup = await _familyRepository.FindPendingInvitationByIdAsync(
            request.InvitationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Invitation), request.InvitationId);

        var (family, invitation) = lookup;

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), userId);

        if (!user.Email.Equals(invitation.Email))
        {
            throw new ForbiddenException(
                "This invitation was sent to a different email address.");
        }

        family.DeclineInvitation(request.InvitationId);
        _familyRepository.Update(family);
    }
}
