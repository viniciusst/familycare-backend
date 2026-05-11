using FamilyCare.Application.Common.Abstractions;
using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.AcceptInvitation;

public sealed class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(120);

        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(_ => dateTimeProvider.TodayUtc)
            .WithMessage("Birth date cannot be in the future.");
    }
}
