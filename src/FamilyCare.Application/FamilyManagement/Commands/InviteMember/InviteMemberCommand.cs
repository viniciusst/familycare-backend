using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Commands.InviteMember;

public sealed record InviteMemberCommand(
    FamilyId FamilyId,
    string Email,
    Role ProposedRole,
    RelationshipType ProposedRelationship,
    int TtlDays = 7)
    : ICommand<InviteMemberResponse>;

public sealed record InviteMemberResponse(InvitationId InvitationId, DateTime ExpiresAt);
