using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Commands.CreateFamily;

public sealed class CreateFamilyCommandHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<CreateFamilyCommand, CreateFamilyResponse>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<CreateFamilyResponse> Handle(
        CreateFamilyCommand request,
        CancellationToken cancellationToken)
    {
        var ownerUserId = _currentUserService.RequireUserId();

        var family = Family.Create(
            request.Name,
            ownerUserId,
            request.OwnerDisplayName,
            request.OwnerBirthDate);

        await _familyRepository.AddAsync(family, cancellationToken);

        var ownerMember = family.Members.First();
        return new CreateFamilyResponse(family.Id, ownerMember.Id);
    }
}
