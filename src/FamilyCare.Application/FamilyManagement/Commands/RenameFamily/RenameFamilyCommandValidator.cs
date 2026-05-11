using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.RenameFamily;

public sealed class RenameFamilyCommandValidator : AbstractValidator<RenameFamilyCommand>
{
    public RenameFamilyCommandValidator()
    {
        RuleFor(x => x.NewName).NotEmpty().MaximumLength(100);
    }
}
