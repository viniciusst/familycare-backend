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

    Task AddAsync(Family family, CancellationToken cancellationToken = default);

    void Update(Family family);

    void Remove(Family family);
}
