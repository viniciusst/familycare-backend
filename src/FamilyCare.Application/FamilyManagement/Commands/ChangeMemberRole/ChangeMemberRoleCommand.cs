using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    Role NewRole)
    : ICommand;
