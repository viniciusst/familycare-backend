using FamilyCare.Application.Authorization;
using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.MedicalHistory.Authorization;

/// <summary>
/// Centralizes the recurring pattern of resolving the current user's membership
/// and checking privacy rules before reading/writing medical data of another member.
/// </summary>
public sealed class MedicalAccessGuard(
    ICurrentUserService currentUserService,
    IMembershipResolver membershipResolver,
    IPrivacyPolicyEvaluator privacyEvaluator)
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMembershipResolver _membershipResolver = membershipResolver;
    private readonly IPrivacyPolicyEvaluator _privacyEvaluator = privacyEvaluator;

    /// <summary>
    /// Ensures the current user can READ data of <paramref name="ownerMemberId"/>
    /// in the given category. Throws ForbiddenException otherwise.
    /// Returns the resolved (FamilyId, requesterMemberId).
    /// </summary>
    public async Task<(FamilyId FamilyId, FamilyMemberId RequesterMemberId)> EnsureCanReadAsync(
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken)
    {
        var (familyId, requesterId) = await ResolveAsync(ownerMemberId, cancellationToken);

        var allowed = await _privacyEvaluator.CanReadAsync(
            familyId, requesterId, ownerMemberId, category, cancellationToken);

        if (!allowed)
        {
            throw new ForbiddenException(
                $"You don't have permission to read this member's {category} data.");
        }

        return (familyId, requesterId);
    }

    /// <summary>
    /// Ensures the current user can WRITE data on behalf of <paramref name="ownerMemberId"/>.
    /// Throws ForbiddenException otherwise. Returns the resolved (FamilyId, requesterMemberId).
    /// </summary>
    public async Task<(FamilyId FamilyId, FamilyMemberId RequesterMemberId)> EnsureCanWriteAsync(
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken)
    {
        var (familyId, requesterId) = await ResolveAsync(ownerMemberId, cancellationToken);

        var allowed = await _privacyEvaluator.CanWriteAsync(
            familyId, requesterId, ownerMemberId, category, cancellationToken);

        if (!allowed)
        {
            throw new ForbiddenException(
                $"You don't have permission to write {category} data for this member.");
        }

        return (familyId, requesterId);
    }

    private async Task<(FamilyId FamilyId, FamilyMemberId RequesterId)> ResolveAsync(
        FamilyMemberId ownerMemberId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var familyId = await _membershipResolver.GetFamilyIdForMemberAsync(
            ownerMemberId, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyMember), ownerMemberId);

        var membership = await _membershipResolver.GetMembershipAsync(
            userId, familyId, cancellationToken)
            ?? throw new ForbiddenException("You are not a member of this family.");

        return (familyId, membership.Id);
    }
}
