using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.CreateFamily;

public sealed record CreateFamilyCommand(
    string Name,
    string OwnerDisplayName,
    DateOnly OwnerBirthDate)
    : ICommand<CreateFamilyResponse>;

public sealed record CreateFamilyResponse(FamilyId FamilyId, FamilyMemberId OwnerMemberId);
