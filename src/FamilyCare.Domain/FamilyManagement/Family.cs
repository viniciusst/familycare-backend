using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement.Events;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Domain.FamilyManagement;

/// <summary>
/// Family aggregate root.
/// Owns its members and invitations. All mutations to members and invitations
/// must go through this root in order to enforce family-level invariants
/// (single owner, role transitions, etc.).
/// </summary>
public sealed class Family : AggregateRoot<FamilyId>
{
    public string Name { get; private set; }
    public UserId OwnerUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<FamilyMember> _members = [];
    public IReadOnlyCollection<FamilyMember> Members => _members.AsReadOnly();

    private readonly List<Invitation> _invitations = [];
    public IReadOnlyCollection<Invitation> Invitations => _invitations.AsReadOnly();

    private Family() : base()
    {
        Name = null!;
    }

    private Family(
        FamilyId id,
        string name,
        UserId ownerUserId,
        DateTime createdAt) : base(id)
    {
        Name = ValidateName(name);
        OwnerUserId = ownerUserId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Creates a new family. The creator becomes the Owner FamilyMember.
    /// </summary>
    public static Family Create(
        string name,
        UserId ownerUserId,
        string ownerDisplayName,
        DateOnly ownerBirthDate)
    {
        var now = DateTime.UtcNow;
        var family = new Family(FamilyId.New(), name, ownerUserId, now);

        var ownerMember = new FamilyMember(
            FamilyMemberId.New(),
            family.Id,
            ownerUserId,
            ownerDisplayName,
            ownerBirthDate,
            Role.Owner,
            RelationshipType.Self,
            now);

        family._members.Add(ownerMember);

        family.RaiseDomainEvent(new FamilyCreatedEvent(family.Id, ownerUserId, family.Name, now));
        family.RaiseDomainEvent(new FamilyMemberAddedEvent(
            family.Id, ownerMember.Id, ownerUserId, Role.Owner, now));

        return family;
    }

    public void Rename(string newName)
    {
        Name = ValidateName(newName);
    }

    /// <summary>Sends an invitation by email.</summary>
    public Invitation InviteMember(
        Email email,
        Role proposedRole,
        RelationshipType proposedRelationship,
        TimeSpan ttl)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (ttl <= TimeSpan.Zero)
        {
            throw new InvalidEntityStateException(
                "family.invitation.invalid_ttl",
                "Invitation TTL must be positive.");
        }

        // Cannot invite an email that already has a pending invitation.
        var pendingForEmail = _invitations
            .FirstOrDefault(i => i.Status == InvitationStatus.Pending && i.Email == email);

        if (pendingForEmail is not null)
        {
            throw new BusinessRuleViolationException(
                "family.invitation.duplicate_pending",
                "There is already a pending invitation for this email.");
        }

        var now = DateTime.UtcNow;
        var invitation = new Invitation(
            InvitationId.New(),
            Id,
            email,
            proposedRole,
            proposedRelationship,
            now,
            now.Add(ttl));

        _invitations.Add(invitation);

        RaiseDomainEvent(new InvitationSentEvent(
            Id, invitation.Id, email.Value, proposedRole, invitation.ExpiresAt, now));

        return invitation;
    }

    /// <summary>
    /// Accepts a pending invitation, creating a new FamilyMember bound to <paramref name="acceptingUserId"/>.
    /// </summary>
    public FamilyMember AcceptInvitation(
        InvitationId invitationId,
        UserId acceptingUserId,
        string displayName,
        DateOnly birthDate)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId)
            ?? throw new EntityNotFoundException(nameof(Invitation), invitationId);

        var now = DateTime.UtcNow;
        invitation.Accept(now);

        var member = new FamilyMember(
            FamilyMemberId.New(),
            Id,
            acceptingUserId,
            displayName,
            birthDate,
            invitation.ProposedRole,
            invitation.ProposedRelationship,
            now);

        _members.Add(member);

        RaiseDomainEvent(new InvitationAcceptedEvent(Id, invitationId, acceptingUserId, now));
        RaiseDomainEvent(new FamilyMemberAddedEvent(
            Id, member.Id, acceptingUserId, member.Role, now));

        return member;
    }

    public void DeclineInvitation(InvitationId invitationId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId)
            ?? throw new EntityNotFoundException(nameof(Invitation), invitationId);

        invitation.Decline(DateTime.UtcNow);
    }

    public void RevokeInvitation(InvitationId invitationId)
    {
        var invitation = _invitations.FirstOrDefault(i => i.Id == invitationId)
            ?? throw new EntityNotFoundException(nameof(Invitation), invitationId);

        var now = DateTime.UtcNow;
        invitation.Revoke(now);
        RaiseDomainEvent(new InvitationRevokedEvent(Id, invitationId, now));
    }

    public void RemoveMember(FamilyMemberId memberId)
    {
        var member = FindMemberOrThrow(memberId);

        if (member.Role == Role.Owner)
        {
            throw new BusinessRuleViolationException(
                "family.member.cannot_remove_owner",
                "The Owner cannot be removed from the family. Transfer ownership first.");
        }

        _members.Remove(member);
        RaiseDomainEvent(new FamilyMemberRemovedEvent(Id, memberId, DateTime.UtcNow));
    }

    public void ChangeMemberRole(FamilyMemberId memberId, Role newRole)
    {
        var member = FindMemberOrThrow(memberId);

        if (member.Role == newRole)
        {
            return;
        }

        if (member.Role == Role.Owner)
        {
            throw new BusinessRuleViolationException(
                "family.member.cannot_demote_owner",
                "Owner role cannot be changed directly. Use TransferOwnership.");
        }

        if (newRole == Role.Owner)
        {
            throw new BusinessRuleViolationException(
                "family.member.cannot_assign_owner",
                "Owner role cannot be assigned directly. Use TransferOwnership.");
        }

        var oldRole = member.Role;
        member.ChangeRole(newRole);
        RaiseDomainEvent(new FamilyMemberRoleChangedEvent(
            Id, memberId, oldRole, newRole, DateTime.UtcNow));
    }

    public void RenameMember(FamilyMemberId memberId, string newDisplayName)
    {
        var member = FindMemberOrThrow(memberId);
        member.Rename(newDisplayName);
    }

    public void ChangeMemberRelationship(FamilyMemberId memberId, RelationshipType relationship)
    {
        var member = FindMemberOrThrow(memberId);
        member.ChangeRelationship(relationship);
    }

    public void ChangeMemberBirthDate(FamilyMemberId memberId, DateOnly newBirthDate)
    {
        var member = FindMemberOrThrow(memberId);

        // Birth date in the future doesn't make sense.
        if (newBirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new InvalidEntityStateException(
                "family.member.invalid_birth_date",
                "Birth date cannot be in the future.");
        }

        member.ChangeBirthDate(newBirthDate);
    }

    public void TransferOwnership(FamilyMemberId newOwnerMemberId)
    {
        var newOwner = FindMemberOrThrow(newOwnerMemberId);

        var currentOwner = _members.First(m => m.Role == Role.Owner);
        if (currentOwner.Id == newOwnerMemberId)
        {
            return;
        }

        currentOwner.ChangeRole(Role.Admin);
        newOwner.ChangeRole(Role.Owner);
        OwnerUserId = newOwner.UserId;

        var now = DateTime.UtcNow;
        RaiseDomainEvent(new FamilyMemberRoleChangedEvent(Id, currentOwner.Id, Role.Owner, Role.Admin, now));
        RaiseDomainEvent(new FamilyMemberRoleChangedEvent(Id, newOwner.Id, newOwner.Role, Role.Owner, now));
    }

    public void ChangePrivacyRule(
        FamilyMemberId memberId,
        DataCategory category,
        VisibilityScope scope,
        IEnumerable<FamilyMemberId>? allowedMemberIds = null)
    {
        var member = FindMemberOrThrow(memberId);

        // Validate that allowed members all belong to this family.
        if (allowedMemberIds is not null)
        {
            foreach (var allowedId in allowedMemberIds)
            {
                if (!_members.Any(m => m.Id == allowedId))
                {
                    throw new BusinessRuleViolationException(
                        "family.privacy_rule.unknown_member",
                        $"Member '{allowedId}' is not part of this family.");
                }
            }
        }

        member.ChangePrivacyRule(category, scope, allowedMemberIds);
        RaiseDomainEvent(new PrivacyRuleChangedEvent(
            Id, memberId, category, scope, DateTime.UtcNow));
    }

    private FamilyMember FindMemberOrThrow(FamilyMemberId memberId)
        => _members.FirstOrDefault(m => m.Id == memberId)
            ?? throw new EntityNotFoundException(nameof(FamilyMember), memberId);

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidEntityStateException(
                "family.name_required",
                "Family name is required.");
        }

        var trimmed = name.Trim();
        if (trimmed.Length > 100)
        {
            throw new InvalidEntityStateException(
                "family.name_too_long",
                "Family name exceeds 100 characters.");
        }

        return trimmed;
    }
}
