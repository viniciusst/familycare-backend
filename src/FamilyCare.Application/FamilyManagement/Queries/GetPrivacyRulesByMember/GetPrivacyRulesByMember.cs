using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetPrivacyRulesByMember;

/// <summary>
/// Lists every privacy rule defined for a specific member.
///
/// The result includes one entry per category that has been explicitly
/// configured. Categories without an explicit rule are not returned —
/// the caller decides how to render them (typically as "Private" by
/// default, matching PrivacyRule.CreateDefault).
///
/// Access control:
/// - The member themself can always read their own rules.
/// - Owner/Admin of the family can read any member's rules.
/// - Other members cannot.
/// </summary>
public sealed record GetPrivacyRulesByMemberQuery(
    FamilyId FamilyId,
    FamilyMemberId MemberId)
    : IQuery<IReadOnlyList<PrivacyRuleDto>>;

public sealed class GetPrivacyRulesByMemberQueryHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetPrivacyRulesByMemberQuery, IReadOnlyList<PrivacyRuleDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<IReadOnlyList<PrivacyRuleDto>> Handle(
        GetPrivacyRulesByMemberQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        var target = family.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new NotFoundException(nameof(FamilyMember), request.MemberId);

        // The data owner reads their own rules. Family admins can read anyone's.
        if (target.Id != requester.Id && !requester.IsAdmin)
        {
            throw new ForbiddenException(
                "You can only view privacy rules for yourself, or as Owner/Admin for other members.");
        }

        return [.. target.PrivacyRules.Select(r => r.ToDto())];
    }
}
