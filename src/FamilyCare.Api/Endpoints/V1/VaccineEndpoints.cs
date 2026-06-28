using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Commands.RegisterVaccine;
using FamilyCare.Application.MedicalHistory.Commands.UpdateVaccineDetails;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetVaccinesByMember;
using FamilyCare.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class VaccineEndpoints
{
    public static IEndpointRouteBuilder MapVaccineEndpoints(this IEndpointRouteBuilder app)
    {
        var memberScoped = app.MapGroup("/api/v1/members/{memberId:guid}/vaccines")
            .WithTags("Vaccines")
            .RequireAuthorization();

        // POST /api/v1/members/{memberId}/vaccines
        memberScoped.MapPost("/", async (
            Guid memberId,
            [FromBody] RegisterVaccineRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RegisterVaccineCommand(
                FamilyMemberId.From(memberId),
                request.Name,
                request.AppliedAt,
                request.Manufacturer,
                request.BatchNumber,
                request.DoseNumber,
                request.NextDoseDue,
                request.Notes);

            var vaccineId = await sender.Send(command, ct);
            return Results.Created($"/api/v1/vaccines/{vaccineId}", new { id = vaccineId });
        })
        .WithName("RegisterVaccine")
        .WithSummary("Registers a vaccine application for a family member.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/members/{memberId}/vaccines
        memberScoped.MapGet("/", async (
            Guid memberId,
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetVaccinesByMemberQuery(FamilyMemberId.From(memberId), page, pageSize),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetVaccinesByMember")
        .WithSummary("Lists vaccines for a member (paginated).")
        .Produces<PagedResult<VaccineDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        var vaccineScoped = app.MapGroup("/api/v1/vaccines")
            .WithTags("Vaccines")
            .RequireAuthorization();

        vaccineScoped.MapPatch("/{id:guid}/details", async (
            Guid id,
            [FromBody] UpdateVaccineDetailsRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new UpdateVaccineDetailsCommand(
                    VaccineId.From(id),
                    request.NewName,
                    request.NewAppliedAt,
                    request.NewManufacturer,
                    request.NewBatchNumber,
                    request.NewDoseNumber,
                    request.NewNextDoseDue,
                    request.NewNotes),
                ct);
            return Results.NoContent();
        })
        .WithName("UpdateVaccineDetails")
        .WithSummary("Updates a vaccine record's details.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }    

    public sealed record RegisterVaccineRequest(
        string Name,
        DateOnly AppliedAt,
        string? Manufacturer = null,
        string? BatchNumber = null,
        int? DoseNumber = null,
        DateOnly? NextDoseDue = null,
        string? Notes = null);
        
    public sealed record UpdateVaccineDetailsRequest(
        string NewName,
        DateOnly NewAppliedAt,
        string? NewManufacturer = null,
        string? NewBatchNumber = null,
        int? NewDoseNumber = null,
        DateOnly? NewNextDoseDue = null,
        string? NewNotes = null);
}
