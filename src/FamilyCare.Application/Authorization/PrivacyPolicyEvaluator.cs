using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.Authorization;

/// <summary>
/// Default privacy policy implementation. Reads the Family aggregate (with members
/// and their PrivacyRules) and applies the visibility rules for read operations.
/// Write operations follow a stricter set of rules.
/// </summary>
public sealed class PrivacyPolicyEvaluator(IFamilyRepository familyRepository)
    : IPrivacyPolicyEvaluator
{
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<bool> CanReadAsync(
        FamilyId familyId,
        FamilyMemberId requesterMemberId,
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken = default)
    {
        var (family, requester, owner) = await LoadAsync(
            familyId, requesterMemberId, ownerMemberId, cancellationToken);

        // Owner of the data always reads its own data.
        if (requester.Id == owner.Id)
        {
            return true;
        }

        var rule = owner.FindPrivacyRule(category)
        ?? PrivacyRule.CreateDefault(owner.Id, category);
        
        return rule.CanBeSeenBy(requester.Id, requester.IsAdmin);
    }

    public async Task<bool> CanWriteAsync(
        FamilyId familyId,
        FamilyMemberId requesterMemberId,
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken = default)
    {
        var (_, requester, owner) = await LoadAsync(
            familyId, requesterMemberId, ownerMemberId, cancellationToken);

        // Self-write always allowed.
        if (requester.Id == owner.Id)
        {
            return true;
        }

        // Family admins can write on behalf of anyone in the family.
        if (requester.IsAdmin)
        {
            return true;
        }

        // Caregivers can write only if the privacy rule explicitly allows them
        // (Custom scope) or the data owner is a Minor.
        if (requester.Role == Role.Caregiver)
        {
            if (owner.IsMinor)
            {
                return true;
            }

            var rule = owner.FindPrivacyRule(category);
            if (rule is not null
                && rule.Scope == VisibilityScope.Custom
                && rule.AllowedMemberIds.Contains(requester.Id))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<(Family family, FamilyMember requester, FamilyMember owner)> LoadAsync(
        FamilyId familyId,
        FamilyMemberId requesterMemberId,
        FamilyMemberId ownerMemberId,
        CancellationToken ct)
    {
        var family = await _familyRepository.GetByIdAsync(familyId, ct)
            ?? throw new NotFoundException(nameof(Family), familyId);

        var requester = family.Members.FirstOrDefault(m => m.Id == requesterMemberId)
            ?? throw new ForbiddenException("Requester is not a member of this family.");

        var owner = family.Members.FirstOrDefault(m => m.Id == ownerMemberId)
            ?? throw new NotFoundException(nameof(FamilyMember), ownerMemberId);

        return (family, requester, owner);
    }
}
