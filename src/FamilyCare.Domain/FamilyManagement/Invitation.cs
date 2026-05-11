using FamilyCare.Domain.Common;
using FamilyCare.Domain.Identity;

namespace FamilyCare.Domain.FamilyManagement;

/// <summary>
/// Invitation for a yet-unknown user (by email) to join a family.
/// Has a TTL and a status workflow.
/// </summary>
public sealed class Invitation : Entity<InvitationId>
{
    public FamilyId FamilyId { get; private set; }
    public Email Email { get; private set; }
    public Role ProposedRole { get; private set; }
    public RelationshipType ProposedRelationship { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private Invitation() : base()
    {
        Email = null!;
    }

    internal Invitation(
        InvitationId id,
        FamilyId familyId,
        Email email,
        Role proposedRole,
        RelationshipType proposedRelationship,
        DateTime createdAt,
        DateTime expiresAt) : base(id)
    {
        if (proposedRole == Role.Owner)
        {
            throw new InvalidEntityStateException(
                "family.invitation.cannot_invite_owner",
                "Cannot invite a member as Owner. The Owner is the family creator.");
        }

        if (expiresAt <= createdAt)
        {
            throw new InvalidEntityStateException(
                "family.invitation.invalid_expiration",
                "Expiration must be after creation date.");
        }

        FamilyId = familyId;
        Email = email;
        ProposedRole = proposedRole;
        ProposedRelationship = proposedRelationship;
        Status = InvitationStatus.Pending;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired(DateTime nowUtc) => nowUtc >= ExpiresAt;

    internal void Accept(DateTime nowUtc)
    {
        EnsurePending();

        if (IsExpired(nowUtc))
        {
            Status = InvitationStatus.Expired;
            throw new BusinessRuleViolationException(
                "family.invitation.expired",
                "Invitation has expired.");
        }

        Status = InvitationStatus.Accepted;
        RespondedAt = nowUtc;
    }

    internal void Decline(DateTime nowUtc)
    {
        EnsurePending();
        Status = InvitationStatus.Declined;
        RespondedAt = nowUtc;
    }

    internal void Revoke(DateTime nowUtc)
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new BusinessRuleViolationException(
                "family.invitation.cannot_revoke",
                "Only pending invitations can be revoked.");
        }

        Status = InvitationStatus.Revoked;
        RespondedAt = nowUtc;
    }

    private void EnsurePending()
    {
        if (Status != InvitationStatus.Pending)
        {
            throw new BusinessRuleViolationException(
                "family.invitation.not_pending",
                $"Invitation is not pending (current status: {Status}).");
        }
    }
}
