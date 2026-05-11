using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.FamilyManagement;

/// <summary>
/// Defines who can see a given DataCategory of data belonging to a FamilyMember.
/// When Scope = Custom, AllowedMemberIds is an ALLOWLIST of members granted access
/// (the owner of the data always has access regardless).
/// </summary>
public sealed class PrivacyRule : Entity<PrivacyRuleId>
{
    public FamilyMemberId FamilyMemberId { get; private set; }
    public DataCategory Category { get; private set; }
    public VisibilityScope Scope { get; private set; }

    private readonly HashSet<FamilyMemberId> _allowedMemberIds = [];
    public IReadOnlyCollection<FamilyMemberId> AllowedMemberIds => _allowedMemberIds;

    private PrivacyRule() : base() { }

    private PrivacyRule(
        PrivacyRuleId id,
        FamilyMemberId memberId,
        DataCategory category,
        VisibilityScope scope,
        IEnumerable<FamilyMemberId>? allowedMemberIds) : base(id)
    {
        FamilyMemberId = memberId;
        Category = category;
        Scope = scope;

        if (allowedMemberIds is not null)
        {
            foreach (var allowed in allowedMemberIds)
            {
                _allowedMemberIds.Add(allowed);
            }
        }

        EnforceCustomScopeInvariant();
    }

    /// <summary>Creates a default rule for a category (Private — owner only).</summary>
    public static PrivacyRule CreateDefault(FamilyMemberId memberId, DataCategory category)
        => new(PrivacyRuleId.New(), memberId, category, VisibilityScope.Private, null);

    public static PrivacyRule Create(
        FamilyMemberId memberId,
        DataCategory category,
        VisibilityScope scope,
        IEnumerable<FamilyMemberId>? allowedMemberIds = null)
        => new(PrivacyRuleId.New(), memberId, category, scope, allowedMemberIds);

    /// <summary>Updates the rule scope and (when Custom) the allowlist.</summary>
    public void Change(VisibilityScope newScope, IEnumerable<FamilyMemberId>? newAllowedMemberIds)
    {
        Scope = newScope;
        _allowedMemberIds.Clear();

        if (newAllowedMemberIds is not null)
        {
            foreach (var id in newAllowedMemberIds)
            {
                _allowedMemberIds.Add(id);
            }
        }

        EnforceCustomScopeInvariant();
    }

    /// <summary>
    /// Returns true if <paramref name="requesterId"/> is allowed to see the data
    /// owned by this rule's FamilyMember, given the requester's role and family-admin status.
    /// </summary>
    public bool CanBeSeenBy(FamilyMemberId requesterId, bool requesterIsAdmin)
    {
        // Owner of the data always sees it.
        if (requesterId == FamilyMemberId)
        {
            return true;
        }

        return Scope switch
        {
            VisibilityScope.Private => false,
            VisibilityScope.FamilyAdmins => requesterIsAdmin,
            VisibilityScope.AllFamily => true,
            VisibilityScope.Custom => _allowedMemberIds.Contains(requesterId),
            _ => false
        };
    }

    private void EnforceCustomScopeInvariant()
    {
        if (Scope != VisibilityScope.Custom && _allowedMemberIds.Count > 0)
        {
            throw new InvalidEntityStateException(
                "family.privacy_rule.invalid_allowlist",
                "AllowedMemberIds can only be populated when Scope is Custom.");
        }
    }
}
