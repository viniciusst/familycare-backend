using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.TransferOwnership;

public sealed class TransferOwnershipCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<TransferOwnershipCommand>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task Handle(TransferOwnershipCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        var requester = family.Members.FirstOrDefault(m => m.UserId == userId)
            ?? throw new ForbiddenException("You are not a member of this family.");

        if (requester.Role != Role.Owner)
        {
            throw new ForbiddenException("Only the current Owner can transfer ownership.");
        }

        family.TransferOwnership(request.NewOwnerMemberId);
        _familyRepository.Update(family);
    }
}
