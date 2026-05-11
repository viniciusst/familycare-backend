using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Mappings;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Domain.FamilyManagement;
using MediatR;

namespace FamilyCare.Application.FamilyManagement.Queries.GetFamilyMembers;

public sealed class GetFamilyMembersQueryHandler(
    ICurrentUserService currentUserService,
    IFamilyRepository familyRepository)
    : IRequestHandler<GetFamilyMembersQuery, IReadOnlyList<FamilyMemberDto>>
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IFamilyRepository _familyRepository = familyRepository;

    public async Task<IReadOnlyList<FamilyMemberDto>> Handle(
        GetFamilyMembersQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.RequireUserId();

        var family = await _familyRepository.GetByIdAsync(request.FamilyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Family), request.FamilyId);

        if (!family.Members.Any(m => m.UserId == userId))
        {
            throw new ForbiddenException("You are not a member of this family.");
        }

        return family.Members.Select(m => m.ToDto()).ToList();
    }
}
