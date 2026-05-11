using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.AcceptInvitation;

public sealed record AcceptInvitationCommand(
    InvitationId InvitationId,
    string DisplayName,
    DateOnly BirthDate)
    : ICommand<AcceptInvitationResponse>;

public sealed record AcceptInvitationResponse(FamilyId FamilyId, FamilyMemberId MemberId);
