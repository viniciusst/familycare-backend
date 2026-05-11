using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.RevokeInvitation;

public sealed record RevokeInvitationCommand(FamilyId FamilyId, InvitationId InvitationId) : ICommand;
