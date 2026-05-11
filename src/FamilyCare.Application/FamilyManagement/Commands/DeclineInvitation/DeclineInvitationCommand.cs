using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.DeclineInvitation;

public sealed record DeclineInvitationCommand(InvitationId InvitationId) : ICommand;
