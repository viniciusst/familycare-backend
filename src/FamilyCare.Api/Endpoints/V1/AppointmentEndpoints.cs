using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.MedicalHistory.Commands.CancelAppointment;
using FamilyCare.Application.MedicalHistory.Commands.CompleteAppointment;
using FamilyCare.Application.MedicalHistory.Commands.RescheduleAppointment;
using FamilyCare.Application.MedicalHistory.Commands.ScheduleAppointment;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetAppointmentsByMember;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.MedicalHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class AppointmentEndpoints
{
    public static IEndpointRouteBuilder MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        // List + create under /members/{memberId}/appointments
        var memberScoped = app.MapGroup("/api/v1/members/{memberId:guid}/appointments")
            .WithTags("Appointments")
            .RequireAuthorization();

        // POST /api/v1/members/{memberId}/appointments
        memberScoped.MapPost("/", async (
            Guid memberId,
            [FromBody] ScheduleAppointmentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new ScheduleAppointmentCommand(
                FamilyMemberId.From(memberId),
                request.ScheduledAt,
                request.Specialty,
                request.DoctorName,
                request.Location,
                request.Notes);

            var appointmentId = await sender.Send(command, ct);
            return Results.Created($"/api/v1/appointments/{appointmentId}", new { id = appointmentId });
        })
        .WithName("ScheduleAppointment")
        .WithSummary("Schedules a new medical appointment for a family member.")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/members/{memberId}/appointments
        memberScoped.MapGet("/", async (
            Guid memberId,
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] AppointmentStatus? status = null,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetAppointmentsByMemberQuery(
                    FamilyMemberId.From(memberId), page, pageSize, status),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetAppointmentsByMember")
        .WithSummary("Lists appointments for a member (paginated, optionally filtered by status).")
        .Produces<PagedResult<AppointmentDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // State transitions under /appointments/{id}
        var appointmentScoped = app.MapGroup("/api/v1/appointments")
            .WithTags("Appointments")
            .RequireAuthorization();

        // POST /api/v1/appointments/{id}/complete
        appointmentScoped.MapPost("/{id:guid}/complete", async (
            Guid id,
            [FromBody] CompleteAppointmentRequest? request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new CompleteAppointmentCommand(
                    AppointmentId.From(id),
                    request?.ClosingNotes),
                ct);
            return Results.NoContent();
        })
        .WithName("CompleteAppointment")
        .WithSummary("Marks an appointment as completed.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/v1/appointments/{id}/cancel
        appointmentScoped.MapPost("/{id:guid}/cancel", async (
            Guid id,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new CancelAppointmentCommand(AppointmentId.From(id)), ct);
            return Results.NoContent();
        })
        .WithName("CancelAppointment")
        .WithSummary("Cancels a scheduled appointment.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/appointments/{id}/reschedule
        appointmentScoped.MapPatch("/{id:guid}/reschedule", async (
            Guid id,
            [FromBody] RescheduleAppointmentRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new RescheduleAppointmentCommand(
                    AppointmentId.From(id), request.NewScheduledAt),
                ct);
            return Results.NoContent();
        })
        .WithName("RescheduleAppointment")
        .WithSummary("Reschedules an appointment to a new date/time.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record ScheduleAppointmentRequest(
        DateTime ScheduledAt,
        string Specialty,
        string DoctorName,
        string? Location = null,
        string? Notes = null);

    public sealed record CompleteAppointmentRequest(string? ClosingNotes);

    public sealed record RescheduleAppointmentRequest(DateTime NewScheduledAt);
}
