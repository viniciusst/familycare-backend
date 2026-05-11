using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Dtos;

public sealed record FamilySummaryDto(
    FamilyId Id,
    string Name,
    UserId OwnerUserId,
    int MemberCount,
    DateTime CreatedAt);

public sealed record FamilyDetailsDto(
    FamilyId Id,
    string Name,
    UserId OwnerUserId,
    DateTime CreatedAt,
    IReadOnlyList<FamilyMemberDto> Members,
    IReadOnlyList<InvitationDto> PendingInvitations);

public sealed record FamilyMemberDto(
    FamilyMemberId Id,
    UserId UserId,
    string DisplayName,
    DateOnly BirthDate,
    Role Role,
    RelationshipType Relationship,
    DateTime JoinedAt,
    IReadOnlyList<PrivacyRuleDto> PrivacyRules);

public sealed record PrivacyRuleDto(
    DataCategory Category,
    VisibilityScope Scope,
    IReadOnlyList<FamilyMemberId> AllowedMemberIds);

public sealed record InvitationDto(
    InvitationId Id,
    string Email,
    Role ProposedRole,
    RelationshipType ProposedRelationship,
    InvitationStatus Status,
    DateTime CreatedAt,
    DateTime ExpiresAt);
