using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.RemoveMember;

public sealed class RemoveMemberCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<RemoveMemberCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        // Self-removal is allowed; otherwise must be admin.
        var target = family.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new NotFoundException(nameof(FamilyMember), request.MemberId);

        if (target.Id != requester.Id && !requester.IsAdmin)
        {
            throw new ForbiddenException(
                "Only Owner or Admin can remove other members. You can remove yourself.");
        }

        family.RemoveMember(request.MemberId);
        _familyRepository.Update(family);
    }
}
