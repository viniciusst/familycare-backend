using FamilyCare.Application.MedicalHistory.Commands.UploadAttachment;
using FamilyCare.Application.MedicalHistory.Dtos;
using FamilyCare.Application.MedicalHistory.Queries.GetAttachmentsByOwner;
using FamilyCare.Domain.MedicalHistory;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class AttachmentEndpoints
{
    // 50 MB upload limit, mirrors the value in UploadAttachmentCommandValidator
    private const long MaxUploadSizeBytes = 50L * 1024 * 1024;

    public static IEndpointRouteBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/attachments")
            .WithTags("Attachments")
            .RequireAuthorization();

        // POST /api/v1/attachments (multipart/form-data)
        group.MapPost("/", async (
            HttpRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Content-Type must be multipart/form-data." });
            }

            var form = await request.ReadFormAsync(ct);

            if (!Enum.TryParse<AttachmentOwnerType>(form["ownerType"], out var ownerType))
            {
                return Results.BadRequest(new { error = "Invalid ownerType." });
            }

            if (!Guid.TryParse(form["ownerEntityId"], out var ownerEntityId))
            {
                return Results.BadRequest(new { error = "Invalid ownerEntityId." });
            }

            var file = form.Files["file"];
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "File is required." });
            }

            if (file.Length > MaxUploadSizeBytes)
            {
                return Results.BadRequest(new { error = $"File exceeds maximum size of {MaxUploadSizeBytes / (1024 * 1024)} MB." });
            }

            await using var stream = file.OpenReadStream();

            var command = new UploadAttachmentCommand(
                ownerType,
                ownerEntityId,
                file.FileName,
                file.ContentType,
                stream,
                file.Length);

            var attachmentId = await sender.Send(command, ct);
            return Results.Created(
                $"/api/v1/attachments/{attachmentId}",
                new { id = attachmentId });
        })
        .WithName("UploadAttachment")
        .WithSummary("Uploads an attachment for an appointment, exam, or vaccine record.")
        .DisableAntiforgery() // multipart from JS/mobile clients without antiforgery token
        .Accepts<IFormFile>("multipart/form-data")
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // GET /api/v1/attachments?ownerType=Appointment&ownerEntityId=...
        group.MapGet("/", async (
            ISender sender,
            [FromQuery] AttachmentOwnerType ownerType,
            [FromQuery] Guid ownerEntityId,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetAttachmentsByOwnerQuery(ownerType, ownerEntityId),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetAttachmentsByOwner")
        .WithSummary("Lists attachments owned by a given medical entity.")
        .Produces<IReadOnlyList<AttachmentDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        return app;
    }
}
