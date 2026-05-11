using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Commands.RegisterExam;
using FamilyCare.Application.MedicalHistory.Commands.UpdateExamResults;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetExamsByMember;
using FamilyCare.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class ExamEndpoints
{
    public static IEndpointRouteBuilder MapExamEndpoints(this IEndpointRouteBuilder app)
    {
        var memberScoped = app.MapGroup("/api/v1/members/{memberId:guid}/exams")
            .WithTags("Exams")
            .RequireAuthorization();

        // POST /api/v1/members/{memberId}/exams
        memberScoped.MapPost("/", async (
            Guid memberId,
            [FromBody] RegisterExamRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new RegisterExamCommand(
                FamilyMemberId.From(memberId),
                request.ExamDate,
                request.ExamType,
                request.Laboratory,
                request.Results,
                request.RequestedBy);

            var examId = await sender.Send(command, ct);
            return Results.Created($"/api/v1/exams/{examId}", new { id = examId });
        })
        .WithName("RegisterExam")
        .WithSummary("Registers a new exam for a family member.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/members/{memberId}/exams
        memberScoped.MapGet("/", async (
            Guid memberId,
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetExamsByMemberQuery(
                    FamilyMemberId.From(memberId), page, pageSize, fromDate, toDate),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetExamsByMember")
        .WithSummary("Lists exams for a member (paginated, optionally filtered by date range).")
        .Produces<PagedResult<ExamDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // PATCH /api/v1/exams/{id}/results
        var examScoped = app.MapGroup("/api/v1/exams")
            .WithTags("Exams")
            .RequireAuthorization();

        examScoped.MapPatch("/{id:guid}/results", async (
            Guid id,
            [FromBody] UpdateExamResultsRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new UpdateExamResultsCommand(ExamId.From(id), request.NewResults),
                ct);
            return Results.NoContent();
        })
        .WithName("UpdateExamResults")
        .WithSummary("Updates the results of an existing exam.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record RegisterExamRequest(
        DateOnly ExamDate,
        string ExamType,
        string? Laboratory = null,
        string? Results = null,
        string? RequestedBy = null);

    public sealed record UpdateExamResultsRequest(string NewResults);
}
