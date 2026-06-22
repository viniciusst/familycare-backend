using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using FluentValidation;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.UpdateMemberDetails;

/// <summary>
/// Updates editable details of a family member (displayName, birthDate,
/// relationship). All fields are optional, but at least one must be
/// provided. Role changes go through ChangeMemberRole; privacy rules
/// through ChangePrivacyRule; this command is for personal data only.
/// </summary>
public sealed record UpdateMemberDetailsCommand(
    FamilyId FamilyId,
    FamilyMemberId MemberId,
    string? DisplayName,
    DateOnly? BirthDate,
    RelationshipType? Relationship)
    : ICommand;

public sealed class UpdateMemberDetailsCommandValidator
    : AbstractValidator<UpdateMemberDetailsCommand>
{
    public UpdateMemberDetailsCommandValidator()
    {
        When(x => x.DisplayName is not null, () =>
        {
            RuleFor(x => x.DisplayName!)
                .NotEmpty()
                .MaximumLength(120);
        });

        When(x => x.BirthDate is not null, () =>
        {
            RuleFor(x => x.BirthDate!.Value)
                .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Birth date cannot be in the future.");
        });

        // At least one field must be provided.
        RuleFor(x => x)
            .Must(x =>
                x.DisplayName is not null ||
                x.BirthDate is not null ||
                x.Relationship is not null)
            .WithMessage("At least one field must be provided to update.");
    }
}

public sealed class UpdateMemberDetailsCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<UpdateMemberDetailsCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(
        UpdateMemberDetailsCommand request,
        CancellationToken cancellationToken)
    {
        var callerUserId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var caller = family.Members.FirstOrDefault(m => m.UserId == callerUserId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        var target = family.Members.FirstOrDefault(m => m.Id == request.MemberId)
            ?? throw new NotFoundException(nameof(FamilyMember), request.MemberId);

        // Authorization: Owner/Admin can edit anyone; regular members
        // can only edit themselves.
        var isOwnerOrAdmin = caller.Role == Role.Owner || caller.Role == Role.Admin;
        var isEditingSelf = caller.Id == target.Id;

        if (!isOwnerOrAdmin && !isEditingSelf)
        {
            throw new ForbiddenException(
                "You can only edit your own member record.");
        }

        if (request.DisplayName is not null)
        {
            family.RenameMember(request.MemberId, request.DisplayName);
        }

        if (request.BirthDate is not null)
        {
            family.ChangeMemberBirthDate(request.MemberId, request.BirthDate.Value);
        }

        if (request.Relationship is not null)
        {
            family.ChangeMemberRelationship(request.MemberId, request.Relationship.Value);
        }

        _familyRepository.Update(family);
    }
}