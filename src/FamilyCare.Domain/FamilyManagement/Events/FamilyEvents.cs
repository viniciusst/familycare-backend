using FamilyCare.Domain.Common;

namespace FamilyCare.Domain.FamilyManagement.Events;

public sealed record FamilyCreatedEvent(
    FamilyId FamilyId,
    UserId OwnerUserId,
    string Name,
    DateTime OccurredOn) : IDomainEvent;

public sealed record FamilyMemberAddedEvent(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    UserId UserId,
    Role Role,
    DateTime OccurredOn) : IDomainEvent;

public sealed record FamilyMemberRemovedEvent(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record FamilyMemberRoleChangedEvent(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    Role OldRole,
    Role NewRole,
    DateTime OccurredOn) : IDomainEvent;

public sealed record InvitationSentEvent(
    FamilyId FamilyId,
    InvitationId InvitationId,
    string Email,
    Role ProposedRole,
    DateTime ExpiresAt,
    DateTime OccurredOn) : IDomainEvent;

public sealed record InvitationAcceptedEvent(
    FamilyId FamilyId,
    InvitationId InvitationId,
    UserId AcceptedByUserId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record InvitationRevokedEvent(
    FamilyId FamilyId,
    InvitationId InvitationId,
    DateTime OccurredOn) : IDomainEvent;

public sealed record PrivacyRuleChangedEvent(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    DataCategory Category,
    VisibilityScope NewScope,
    DateTime OccurredOn) : IDomainEvent;
