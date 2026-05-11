using FamilyCare.Application.FamilyManagement.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using Microsoft.EntityFrameworkCore;

namespace FamilyCare.Infrastructure.Persistence.Repositories.FamilyManagement;

public sealed class MembershipResolver(FamilyCareDbContext dbContext) : IMembershipResolver
{
    private readonly FamilyCareDbContext _dbContext = dbContext;

    public Task<FamilyMember?> GetMembershipAsync(
        UserId userId,
        FamilyId familyId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.FamilyMembers
            .Include(m => m.PrivacyRules)
            .FirstOrDefaultAsync(
                m => m.UserId == userId && m.FamilyId == familyId,
                cancellationToken);
    }

    public async Task<FamilyId?> GetFamilyIdForMemberAsync(
        FamilyMemberId memberId,
        CancellationToken cancellationToken = default)
    {
        var member = await _dbContext.FamilyMembers
            .AsNoTracking()
            .Where(m => m.Id == memberId)
            .Select(m => new { m.FamilyId })
            .FirstOrDefaultAsync(cancellationToken);

        return member?.FamilyId;
    }
}
