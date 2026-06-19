using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FamilyCare.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.FamilyManagement;

public sealed class FamilyRepository(FamilyCareDbContext dbContext) : IFamilyRepository
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<Family?> GetByIdAsync(FamilyId id, CancellationToken cancellationToken = default)
        => _dbContext.Families
            .Include(f => f.Members)
                .ThenInclude(m => m.PrivacyRules)
            .Include(f => f.Invitations)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<PagedResult<Family>> GetByUserIdAsync(
        UserId userId,
        PagedRequest pagination,
        CancellationToken cancellationToken = default)
    {
        // Find family ids the user belongs to
        var familyIdsQuery = _dbContext.FamilyMembers
            .Where(m => m.UserId == userId)
            .Select(m => m.FamilyId)
            .Distinct();

        var totalCount = await familyIdsQuery.CountAsync(cancellationToken);

        if (totalCount == 0)
        {
            return new PagedResult<Family>([], pagination.NormalizedPage, pagination.NormalizedPageSize, 0);
        }

        var pageOfIds = await familyIdsQuery
            .OrderBy(id => id)
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        var families = await _dbContext.Families
            .Include(f => f.Members)
                .ThenInclude(m => m.PrivacyRules)
            .Include(f => f.Invitations)
            .Where(f => pageOfIds.Contains(f.Id))
            .ToListAsync(cancellationToken);

        return new PagedResult<Family>(
            families,
            pagination.NormalizedPage,
            pagination.NormalizedPageSize,
            totalCount);
    }

    public async Task<(Family Family, Invitation Invitation)?> FindPendingInvitationByIdAsync(
        InvitationId invitationId,
        CancellationToken cancellationToken = default)
    {
        var invitation = await _dbContext.Invitations
            .FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);

        if (invitation is null)
        {
            return null;
        }

        var family = await _dbContext.Families
            .Include(f => f.Members)
                .ThenInclude(m => m.PrivacyRules)
            .Include(f => f.Invitations)
            .FirstOrDefaultAsync(f => f.Id == invitation.FamilyId, cancellationToken);

        if (family is null)
        {
            return null;
        }

        // Re-find inside the loaded family aggregate so the entity is tracked together.
        var trackedInvitation = family.Invitations.First(i => i.Id == invitationId);
        return (family, trackedInvitation);
    }

    public async Task<(Family Family, Invitation Invitation)?> FindInvitationByIdAsync(
        InvitationId invitationId,
        CancellationToken cancellationToken = default)
    {
        // Same pattern as FindPendingInvitationByIdAsync but without status filter.
        var family = await _dbContext.Families
            .Include(f => f.Invitations)
            .FirstOrDefaultAsync(
                f => f.Invitations.Any(i => i.Id == invitationId),
                cancellationToken);

        if (family is null)
        { 
            return null;
        }

        var invitation = family.Invitations.First(i => i.Id == invitationId);
        return (family, invitation);
    }

    public async Task<PagedResult<(Family Family, Invitation Invitation)>> GetInvitationsByEmailAsync(
        string email,
        InvitationStatus? status,
        PagedRequest pagination,
        CancellationToken cancellationToken = default)
    {
        // Email VO normalizes input on creation, so we wrap the search term
        // in an Email instance and compare VO-to-VO. EF Core's value converter
        // handles the SQL translation transparently.
        var searchEmail = Email.Create(email);

        var query = _dbContext.Families
            .SelectMany(
                f => f.Invitations,
                (f, i) => new { Family = f, Invitation = i })
            .Where(x => x.Invitation.Email == searchEmail);

        if (status.HasValue)
        {
            query = query.Where(x => x.Invitation.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.Invitation.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(x => new ValueTuple<Family, Invitation>(x.Family, x.Invitation))
            .ToListAsync(cancellationToken);

        return new PagedResult<(Family, Invitation)>(
            items, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task<PagedResult<Invitation>> GetInvitationsByFamilyAsync(
        FamilyId familyId,
        InvitationStatus? status,
        PagedRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var family = await _dbContext.Families
            .Include(f => f.Invitations)
            .FirstOrDefaultAsync(f => f.Id == familyId, cancellationToken);

        if (family is null)
        {
            return new PagedResult<Invitation>([], pagination.Page, pagination.PageSize, 0);
        }

        var query = family.Invitations.AsEnumerable();
        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        var ordered = query.OrderByDescending(i => i.CreatedAt).ToList();
        var totalCount = ordered.Count;

        var items = ordered
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .ToList();

        return new PagedResult<Invitation>(
            items, pagination.Page, pagination.PageSize, totalCount);
    }

    public async Task AddAsync(Family family, CancellationToken cancellationToken = default)
        => await _dbContext.Families.AddAsync(family, cancellationToken);

    public void Update(Family family) => _dbContext.Families.Update(family);

    public void Remove(Family family) => _dbContext.Families.Remove(family);
}
