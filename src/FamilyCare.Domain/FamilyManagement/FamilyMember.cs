using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.FamilyManagement;

/// <summary>
/// A member of a Family. Bound to a User (auth identity) and carries
/// role + relationship within the family context.
/// </summary>
public sealed class FamilyMember : Entity<FamilyMemberId>
{
    public FamilyId FamilyId { get; private set; }
    public UserId UserId { get; private set; }
    public string DisplayName { get; private set; }
    public DateOnly BirthDate { get; private set; }
    public Role Role { get; private set; }
    public RelationshipType Relationship { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private readonly List<PrivacyRule> _privacyRules = [];
    public IReadOnlyCollection<PrivacyRule> PrivacyRules => _privacyRules.AsReadOnly();

    private FamilyMember() : base()
    {
        DisplayName = null!;
    }

    internal FamilyMember(
        FamilyMemberId id,
        FamilyId familyId,
        UserId userId,
        string displayName,
        DateOnly birthDate,
        Role role,
        RelationshipType relationship,
        DateTime joinedAt) : base(id)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new InvalidEntityStateException(
                "family.member.display_name_required",
                "Display name is required.");
        }

        if (displayName.Length > 120)
        {
            throw new InvalidEntityStateException(
                "family.member.display_name_too_long",
                "Display name exceeds 120 characters.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (birthDate > today)
        {
            throw new InvalidEntityStateException(
                "family.member.invalid_birth_date",
                "Birth date cannot be in the future.");
        }

        FamilyId = familyId;
        UserId = userId;
        DisplayName = displayName.Trim();
        BirthDate = birthDate;
        Role = role;
        Relationship = relationship;
        JoinedAt = joinedAt;

        // Seed default privacy rules (Private) for every category.
        foreach (var category in Enum.GetValues<DataCategory>())
        {
            _privacyRules.Add(PrivacyRule.CreateDefault(id, category));
        }
    }

    public bool IsAdmin => Role is Role.Owner or Role.Admin;

    public bool IsMinor => Role == Role.Minor;

    internal void ChangeRole(Role newRole)
    {
        Role = newRole;
    }

    internal void Rename(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            throw new InvalidEntityStateException(
                "family.member.display_name_required",
                "Display name is required.");
        }

        DisplayName = newDisplayName.Trim();
    }

    internal void ChangeRelationship(RelationshipType relationship)
    {
        Relationship = relationship;
    }

    internal void ChangePrivacyRule(
        DataCategory category,
        VisibilityScope newScope,
        IEnumerable<FamilyMemberId>? newAllowedMemberIds)
    {
        var rule = _privacyRules.FirstOrDefault(r => r.Category == category);
        if (rule is null)
        {
            rule = PrivacyRule.Create(Id, category, newScope, newAllowedMemberIds);
            _privacyRules.Add(rule);
            return;
        }

        rule.Change(newScope, newAllowedMemberIds);
    }

    public PrivacyRule? FindPrivacyRule(DataCategory category)
        => _privacyRules.FirstOrDefault(r => r.Category == category);
}
