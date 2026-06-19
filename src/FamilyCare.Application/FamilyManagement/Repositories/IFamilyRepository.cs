using FamilyCare.Application.Common.Pagination;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Repositories;

public interface IFamilyRepository
{
    /// <summary>Loads the full Family aggregate (members + invitations + privacy rules).</summary>
    Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default);

    /// <summary>Returns families the user belongs to (paginated).</summary>
    Task<PagedResult<Family>> GetByUserIdAsync(
        UserId userId,
        PagedRequest pagination,
        CancellationToken cancellationToken = default);

    /// <summary>Looks up a pending invitation across all families for a given email.</summary>
    Task<(Family Family, Invitation Invitation)?> FindPendingInvitationByIdAsync(
        InvitationId invitationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up an invitation by id without status filter, returning the
    /// owning family. Used when the caller needs to inspect full state.
    /// </summary>
    Task<(Family Family, Invitation Invitation)?> FindInvitationByIdAsync(
        InvitationId invitationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists invitations matching an email address (case-insensitive).
    /// Used to populate the recipient's inbox.
    /// </summary>
    Task<PagedResult<(Family Family, Invitation Invitation)>> GetInvitationsByEmailAsync(
        string email,
        InvitationStatus? status,
        PagedRequest pagination,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists invitations of a given family. Used by owners/admins.
    /// </summary>
    Task<PagedResult<Invitation>> GetInvitationsByFamilyAsync(
        FamilyId familyId,
        InvitationStatus? status,
        PagedRequest pagination,
        CancellationToken cancellationToken = default);

    Task AddAsync(Family family, CancellationToken cancellationToken = default);

    void Update(Family family);

    void Remove(Family family);
}
