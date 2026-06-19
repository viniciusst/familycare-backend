using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Mappings;

internal static class FamilyMappers
{
    public static FamilySummaryDto ToSummaryDto(this Family family)
        => new(family.Id, family.Name, family.OwnerUserId, family.Members.Count, family.CreatedAt);

    public static FamilyDetailsDto ToDetailsDto(this Family family)
        => new(
            family.Id,
            family.Name,
            family.OwnerUserId,
            family.CreatedAt,
            family.Members.Select(ToDto).ToList(),
            family.Invitations
                .Where(i => i.Status == InvitationStatus.Pending)
                .Select(ToDto)
                .ToList());

    public static FamilyMemberDto ToDto(this FamilyMember member)
        => new(
            member.Id,
            member.UserId,
            member.DisplayName,
            member.BirthDate,
            member.Role,
            member.Relationship,
            member.JoinedAt,
            member.PrivacyRules.Select(ToDto).ToList());

    public static PrivacyRuleDto ToDto(this PrivacyRule rule)
        => new(rule.Category, rule.Scope, [.. rule.AllowedMemberIds]);

    public static InvitationDto ToDto(this Invitation invitation)
        => new(
            invitation.Id,
            invitation.Email.Value,
            invitation.ProposedRole,
            invitation.ProposedRelationship,
            invitation.Status,
            invitation.CreatedAt,
            invitation.ExpiresAt);
    public static InvitationDetailsDto ToDetailsDto(
        this Invitation invitation, FamilyId familyId, string familyName) =>
        new(
            invitation.Id,
            familyId,
            familyName,
            invitation.Email.Value,
            invitation.ProposedRole,
            invitation.ProposedRelationship,
            invitation.Status,
            invitation.CreatedAt,
            invitation.ExpiresAt);
}
