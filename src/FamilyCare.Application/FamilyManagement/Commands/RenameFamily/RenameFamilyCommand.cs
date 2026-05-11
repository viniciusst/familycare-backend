using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Commands.RenameFamily;

public sealed record RenameFamilyCommand(FamilyId FamilyId, string NewName) : ICommand;
