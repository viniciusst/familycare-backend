using FamilyCare.Application.Common.Abstractions;
using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.CreateFamily;

public sealed class CreateFamilyCommandValidator : AbstractValidator<CreateFamilyCommand>
{
    public CreateFamilyCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.OwnerDisplayName)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(x => x.OwnerBirthDate)
            .LessThanOrEqualTo(_ => dateTimeProvider.TodayUtc)
            .WithMessage("Birth date cannot be in the future.");
    }
}
