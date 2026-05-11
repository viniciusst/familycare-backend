using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.RemoveMember;

public sealed record RemoveMemberCommand(FamilyId FamilyId, FamilyMemberId MemberId) : ICommand;
