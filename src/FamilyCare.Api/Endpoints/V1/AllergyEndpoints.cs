using FamilyCare.Application.MedicalHistory.Commands.ChangeAllergySeverity;
using FamilyCare.Application.MedicalHistory.Commands.RegisterAllergy;
using FamilyCare.Application.MedicalHistory.Commands.UpdateAllergyDetails;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetAllergiesByMember;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class AllergyEndpoints
{
    public static IEndpointRouteBuilder MapAllergyEndpoints(this IEndpointRouteBuilder app)
    {
        var memberScoped = app.MapGroup("/api/v1/members/{memberId:guid}/allergies")
            .WithTags("Allergies")
            .RequireAuthorization();

        // POST /api/v1/members/{memberId}/allergies
        memberScoped.MapPost("/", async (
            Guid memberId,
            [FromBody] RegisterAllergyRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RegisterAllergyCommand(
                FamilyMemberId.From(memberId),
                request.Substance,
                request.Severity,
                request.Reaction,
                request.FirstObservedAt);

            var allergyId = await sender.Send(command, ct);
            return Results.Created($"/api/v1/allergies/{allergyId}", new { id = allergyId });
        })
        .WithName("RegisterAllergy")
        .WithSummary("Registers an allergy for a family member.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/members/{memberId}/allergies
        memberScoped.MapGet("/", async (
            Guid memberId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetAllergiesByMemberQuery(FamilyMemberId.From(memberId)),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetAllergiesByMember")
        .WithSummary("Lists allergies for a member.")
        .Produces<IReadOnlyList<AllergyDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // PATCH /api/v1/allergies/{id}/severity
        var allergyScoped = app.MapGroup("/api/v1/allergies")
            .WithTags("Allergies")
            .RequireAuthorization();

        allergyScoped.MapPatch("/{id:guid}/severity", async (
            Guid id,
            [FromBody] ChangeAllergySeverityRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new ChangeAllergySeverityCommand(AllergyId.From(id), request.NewSeverity),
                ct);
            return Results.NoContent();
        })
        .WithName("ChangeAllergySeverity")
        .WithSummary("Updates the severity of an existing allergy.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        allergyScoped.MapPatch("/{id:guid}/details", async (
            Guid id,
            [FromBody] UpdateAllergyDetailsRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new UpdateAllergyDetailsCommand(
                    AllergyId.From(id),
                    request.NewSubstance,
                    request.NewReaction,
                    request.NewFirstObservedAt),
                ct);
            return Results.NoContent();
        })
        .WithName("UpdateAllergyDetails")
        .WithSummary("Updates an allergy's details.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record RegisterAllergyRequest(
        string Substance,
        AllergySeverity Severity,
        string? Reaction = null,
        DateOnly? FirstObservedAt = null);

    public sealed record UpdateAllergyDetailsRequest(
        string NewSubstance,
        string? NewReaction = null,
        DateOnly? NewFirstObservedAt = null);

    public sealed record ChangeAllergySeverityRequest(AllergySeverity NewSeverity);
}
