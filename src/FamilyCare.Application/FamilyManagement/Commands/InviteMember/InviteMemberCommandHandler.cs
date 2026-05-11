using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.InviteMember;

public sealed class InviteMemberCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<InviteMemberCommand, InviteMemberResponse>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<InviteMemberResponse> Handle(
        InviteMemberCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        if (!requester.IsAdmin)
        {
            throw new ForbiddenException("Only Owner or Admin can invite new members.");
        }

        var email = Email.Create(request.Email);
        var ttl = TimeSpan.FromDays(request.TtlDays);

        var invitation = family.InviteMember(
            email,
            request.ProposedRole,
            request.ProposedRelationship,
            ttl);

        _familyRepository.Update(family);

        return new InviteMemberResponse(invitation.Id, invitation.ExpiresAt);
    }
}
