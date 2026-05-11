using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Abstractions;

/// <summary>
/// Resolves which FamilyMember corresponds to a given (UserId, FamilyId) pair,
/// or finds the family that owns a target member.
/// </summary>
public interface IMembershipResolver
{
    /// <summary>
    /// Returns the FamilyMember representing <paramref name="userId"/> in
    /// <paramref name="familyId"/>, or null if the user is not a member.
    /// </summary>
    Task<FamilyMember?> GetMembershipAsync(
        UserId userId,
        FamilyId familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the FamilyId that contains <paramref name="memberId"/>.
    /// </summary>
    Task<FamilyId?> GetFamilyIdForMemberAsync(
        FamilyMemberId memberId,
        CancellationToken cancellationToken = default);
}
