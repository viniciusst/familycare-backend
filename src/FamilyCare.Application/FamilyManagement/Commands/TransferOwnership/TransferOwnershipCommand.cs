using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.TransferOwnership;

public sealed record TransferOwnershipCommand(
    FamilyId FamilyId,
    FamilyMemberId NewOwnerMemberId)
    : ICommand;
