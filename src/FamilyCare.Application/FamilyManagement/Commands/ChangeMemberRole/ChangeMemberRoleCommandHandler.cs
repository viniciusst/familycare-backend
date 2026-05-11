using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangeMemberRole;

public sealed class ChangeMemberRoleCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<ChangeMemberRoleCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(ChangeMemberRoleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        if (requester.Role != Role.Owner)
        {
            throw new ForbiddenException("Only the Owner can change member roles.");
        }

        family.ChangeMemberRole(request.MemberId, request.NewRole);
        _familyRepository.Update(family);
    }
}
