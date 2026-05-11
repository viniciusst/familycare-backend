using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.Authorization;

/// <summary>
/// Evaluates whether a requesting member can access data of another member
/// within a family, given the data category and the privacy rules in place.
/// </summary>
public interface IPrivacyPolicyEvaluator
{
    /// <summary>
    /// Returns true if <paramref name="requesterMemberId"/> can read data of
    /// <paramref name="ownerMemberId"/> for the given <paramref name="category"/>.
    /// Throws ForbiddenException via callers when access must be denied.
    /// </summary>
    Task<bool> CanReadAsync(
        FamilyId familyId,
        FamilyMemberId requesterMemberId,
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if <paramref name="requesterMemberId"/> can write/modify data of
    /// <paramref name="ownerMemberId"/> for the given <paramref name="category"/>.
    /// Write rules: owner of the data, or family Owner/Admin, or Caregiver explicitly allowed.
    /// </summary>
    Task<bool> CanWriteAsync(
        FamilyId familyId,
        FamilyMemberId requesterMemberId,
        FamilyMemberId ownerMemberId,
        DataCategory category,
        CancellationToken cancellationToken = default);
}
