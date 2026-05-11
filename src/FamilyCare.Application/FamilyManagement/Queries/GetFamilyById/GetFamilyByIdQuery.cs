using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Domain.Common;

namespace FamilyCare.Application.FamilyManagement.Queries.GetFamilyById;

public sealed record GetFamilyByIdQuery(FamilyId FamilyId) : IQuery<FamilyDetailsDto>;
