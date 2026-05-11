using FamilyCare.Application.MedicalHistory.Commands.RegisterChronicCondition;
using FamilyCare.Application.MedicalHistory.Commands.ResolveChronicCondition;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetChronicConditionsByMember;
using FamilyCare.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class ChronicConditionEndpoints
{
    public static IEndpointRouteBuilder MapChronicConditionEndpoints(this IEndpointRouteBuilder app)
    {
        var memberScoped = app.MapGroup("/api/v1/members/{memberId:guid}/chronic-conditions")
            .WithTags("Chronic Conditions")
            .RequireAuthorization();

        // POST /api/v1/members/{memberId}/chronic-conditions
        memberScoped.MapPost("/", async (
            Guid memberId,
            [FromBody] RegisterChronicConditionRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RegisterChronicConditionCommand(
                FamilyMemberId.From(memberId),
                request.Name,
                request.DiagnosedAt,
                request.Notes);

            var conditionId = await sender.Send(command, ct);
            return Results.Created(
                $"/api/v1/chronic-conditions/{conditionId}",
                new { id = conditionId });
        })
        .WithName("RegisterChronicCondition")
        .WithSummary("Registers a chronic condition for a family member.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/members/{memberId}/chronic-conditions
        memberScoped.MapGet("/", async (
            Guid memberId,
            ISender sender,
            [FromQuery] bool? activeOnly = null,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetChronicConditionsByMemberQuery(
                    FamilyMemberId.From(memberId), activeOnly),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetChronicConditionsByMember")
        .WithSummary("Lists chronic conditions for a member (optionally filtered by active status).")
        .Produces<IReadOnlyList<ChronicConditionDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // POST /api/v1/chronic-conditions/{id}/resolve
        var conditionScoped = app.MapGroup("/api/v1/chronic-conditions")
            .WithTags("Chronic Conditions")
            .RequireAuthorization();

        conditionScoped.MapPost("/{id:guid}/resolve", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new ResolveChronicConditionCommand(ChronicConditionId.From(id)),
                ct);
            return Results.NoContent();
        })
        .WithName("ResolveChronicCondition")
        .WithSummary("Marks a chronic condition as resolved (no longer active).")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record RegisterChronicConditionRequest(
        string Name,
        DateOnly DiagnosedAt,
        string? Notes = null);
}
