using FamilyCare.Domain.FamilyManagement;
using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.ChangePrivacyRule;

public sealed class ChangePrivacyRuleCommandValidator : AbstractValidator<ChangePrivacyRuleCommand>
{
    public ChangePrivacyRuleCommandValidator()
    {
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.NewScope).IsInEnum();

        // AllowedMemberIds only meaningful when scope == Custom.
        When(x => x.NewScope == VisibilityScope.Custom, () =>
        {
            RuleFor(x => x.AllowedMemberIds)
                .NotNull().WithMessage("AllowedMemberIds is required when scope is Custom.")
                .Must(ids => ids != null && ids.Count > 0)
                .WithMessage("AllowedMemberIds must contain at least one member when scope is Custom.");
        });

        When(x => x.NewScope != VisibilityScope.Custom, () =>
        {
            RuleFor(x => x.AllowedMemberIds)
                .Must(ids => ids == null || ids.Count == 0)
                .WithMessage("AllowedMemberIds must be empty when scope is not Custom.");
        });
    }
}
