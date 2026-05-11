using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangeMemberRole;

public sealed class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(x => x.NewRole).IsInEnum();
    }
}
