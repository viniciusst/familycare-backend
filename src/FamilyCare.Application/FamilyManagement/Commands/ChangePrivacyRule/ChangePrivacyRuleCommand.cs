using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangePrivacyRule;

public sealed record ChangePrivacyRuleCommand(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    DataCategory Category,
    VisibilityScope NewScope,
    IReadOnlyCollection<FamilyMemberId>? AllowedMemberIds = null)
    : ICommand;
