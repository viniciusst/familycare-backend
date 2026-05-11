using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Queries.GetFamilyMembers;

public sealed record GetFamilyMembersQuery(FamilyId FamilyId)
    : IQuery<IReadOnlyList<FamilyMemberDto>>;
