using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangePrivacyRule;

public sealed class ChangePrivacyRuleCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<ChangePrivacyRuleCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(ChangePrivacyRuleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        var target = family.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new NotFoundException(nameof(FamilyMember), request.MemberId);

        // Owner of the data may always edit their own privacy.
        // Admins/Owner of family may edit on behalf of others (e.g. a Minor).
        if (target.Id != requester.Id && !requester.IsAdmin)
        {
            throw new ForbiddenException(
                "You can only change privacy rules for yourself, or as Owner/Admin for other members.");
        }

        family.ChangePrivacyRule(
            request.MemberId,
            request.Category,
            request.NewScope,
            request.AllowedMemberIds);

        _familyRepository.Update(family);
    }
}
