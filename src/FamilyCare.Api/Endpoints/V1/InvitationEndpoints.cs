using FamilyCare.Application.Common.Pagination;
using FamilyCare.Application.FamilyManagement.Commands.AcceptInvitation;
using FamilyCare.Application.FamilyManagement.Commands.DeclineInvitation;
using FamilyCare.Application.FamilyManagement.Commands.InviteMember;
using FamilyCare.Application.FamilyManagement.Commands.RevokeInvitation;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Queries.GetInvitationById;
using FamilyCare.Application.FamilyManagement.Queries.GetMyInvitations;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class InvitationEndpoints
{
    public static IEndpointRouteBuilder MapInvitationEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoints under /api/v1/families/{familyId}/invitations
        var familyScoped = app.MapGroup("/api/v1/families/{familyId:guid}/invitations")
            .WithTags("Invitations")
            .RequireAuthorization();

        // POST /api/v1/families/{familyId}/invitations
        familyScoped.MapPost("/", async (
            Guid familyId,
            [FromBody] InviteMemberRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new InviteMemberCommand(
                FamilyId.From(familyId),
                request.Email,
                request.ProposedRole,
                request.ProposedRelationship,
                request.TtlDays);

            var response = await sender.Send(command, ct);
            return Results.Created(
                $"/api/v1/invitations/{response.InvitationId}",
                response);
        })
        .WithName("InviteMember")
        .WithSummary("Sends an invitation by email. Only Owner/Admin can invite.")
        .Produces<InviteMemberResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/families/{familyId}/invitations/{invitationId}
        familyScoped.MapDelete("/{invitationId:guid}", async (
            Guid familyId,
            Guid invitationId,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new RevokeInvitationCommand(
                    FamilyId.From(familyId),
                    InvitationId.From(invitationId)),
                ct);
            return Results.NoContent();
        })
        .WithName("RevokeInvitation")
        .WithSummary("Revokes a pending invitation. Only Owner/Admin can do this.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // Endpoints under /api/v1/invitations (accept/decline are user-scoped, not family-scoped)
        var userScoped = app.MapGroup("/api/v1/invitations")
            .WithTags("Invitations")
            .RequireAuthorization();

        // POST /api/v1/invitations/{invitationId}/accept
        userScoped.MapPost("/{invitationId:guid}/accept", async (
            Guid invitationId,
            [FromBody] AcceptInvitationRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var command = new AcceptInvitationCommand(
                InvitationId.From(invitationId),
                request.DisplayName,
                request.BirthDate);

            var response = await sender.Send(command, ct);
            return Results.Ok(response);
        })
        .WithName("AcceptInvitation")
        .WithSummary("Accepts a pending invitation and joins the family.")
        .Produces<AcceptInvitationResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/v1/invitations/{invitationId}/decline
        userScoped.MapPost("/{invitationId:guid}/decline", async (
            Guid invitationId,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(new DeclineInvitationCommand(InvitationId.From(invitationId)), ct);
            return Results.NoContent();
        })
        .WithName("DeclineInvitation")
        .WithSummary("Declines a pending invitation.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
        // Add inside MapInvitationEndpoints, alongside accept/decline:

        // GET /api/v1/invitations  (current user's inbox)
        app.MapGet("/api/v1/invitations", async (
            ISender sender,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] InvitationStatus? status = null,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetMyInvitationsQuery(page, pageSize, status), ct);
            return Results.Ok(result);
        })
        .WithTags("Invitations")
        .WithName("GetMyInvitations")
        .WithSummary("Lists invitations addressed to the authenticated user (by email).")
        .RequireAuthorization()
        .Produces<PagedResult<InvitationDetailsDto>>();

        // GET /api/v1/invitations/{invitationId}
        app.MapGet("/api/v1/invitations/{invitationId:guid}", async (
            Guid invitationId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetInvitationByIdQuery(InvitationId.From(invitationId)), ct);
            return Results.Ok(result);
        })
        .WithTags("Invitations")
        .WithName("GetInvitationById")
        .WithSummary("Returns a single invitation. Visible to the invitee or to Owner/Admin.")
        .RequireAuthorization()
        .Produces<InvitationDetailsDto>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record InviteMemberRequest(
        string Email,
        Role ProposedRole,
        RelationshipType ProposedRelationship,
        int TtlDays = 7);

    public sealed record AcceptInvitationRequest(
        string DisplayName,
        DateOnly BirthDate);
}
