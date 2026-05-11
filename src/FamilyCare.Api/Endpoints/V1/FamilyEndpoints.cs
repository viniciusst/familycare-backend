using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Commands.CreateFamily;
using FamilyCare.Application.FamilyManagement.Commands.RenameFamily;
using FamilyCare.Application.FamilyManagement.Commands.TransferOwnership;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Queries.GetFamilyById;
using FamilyCare.Application.FamilyManagement.Queries.GetMyFamilies;
using FamilyCare.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class FamilyEndpoints
{
    public static IEndpointRouteBuilder MapFamilyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/families")
            .WithTags("Families")
            .RequireAuthorization();

        // POST /api/v1/families
        group.MapPost("/", async (
            [FromBody] CreateFamilyCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(command, ct);
            return Results.Created($"/api/v1/families/{response.FamilyId}", response);
        })
        .WithName("CreateFamily")
        .WithSummary("Creates a new family with the current user as Owner.")
        .Produces<CreateFamilyResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem();

        // GET /api/v1/families
        group.MapGet("/", async (
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(new GetMyFamiliesQuery(page, pageSize), ct);
            return Results.Ok(result);
        })
        .WithName("GetMyFamilies")
        .WithSummary("Returns families the authenticated user belongs to (paginated).")
        .Produces<PagedResult<FamilySummaryDto>>();

        // GET /api/v1/families/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(new GetFamilyByIdQuery(FamilyId.From(id)), ct);
            return Results.Ok(result);
        })
        .WithName("GetFamilyById")
        .WithSummary("Returns full details of a family the user is a member of.")
        .Produces<FamilyDetailsDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/families/{id}
        group.MapPatch("/{id:guid}", async (
            Guid id,
            [FromBody] RenameFamilyRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new RenameFamilyCommand(FamilyId.From(id), request.NewName), ct);
            return Results.NoContent();
        })
        .WithName("RenameFamily")
        .WithSummary("Renames a family. Only Owner or Admin can perform this action.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // POST /api/v1/families/{id}/transfer-ownership
        group.MapPost("/{id:guid}/transfer-ownership", async (
            Guid id,
            [FromBody] TransferOwnershipRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new TransferOwnershipCommand(
                    FamilyId.From(id),
                    FamilyMemberId.From(request.NewOwnerMemberId)),
                ct);
            return Results.NoContent();
        })
        .WithName("TransferOwnership")
        .WithSummary("Transfers ownership of the family to another member. Only the current Owner can do this.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record RenameFamilyRequest(string NewName);
    public sealed record TransferOwnershipRequest(Guid NewOwnerMemberId);
}
