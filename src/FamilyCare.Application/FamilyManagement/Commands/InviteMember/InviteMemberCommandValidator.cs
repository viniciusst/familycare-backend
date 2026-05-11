using FluentValidation;

namespace FamilyCare.Application.FamilyManagement.Commands.InviteMember;

public sealed class InviteMemberCommandValidator : AbstractValidator<InviteMemberCommand>
{
    public InviteMemberCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.ProposedRole).IsInEnum();
        RuleFor(x => x.ProposedRelationship).IsInEnum();
        RuleFor(x => x.TtlDays).InclusiveBetween(1, 30);
    }
}
