using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.RenameFamily;

public sealed class RenameFamilyCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<RenameFamilyCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(RenameFamilyCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        if (!requester.IsAdmin)
        {
            throw new ForbiddenException("Only Owner or Admin can rename the family.");
        }

        family.Rename(request.NewName);
        _familyRepository.Update(family);
    }
}
