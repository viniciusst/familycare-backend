using FamilyCare.Application.FamilyManagement.Commands.ChangeMemberRole;
using FamilyCare.Application.FamilyManagement.Commands.ChangePrivacyRule;
using FamilyCare.Application.FamilyManagement.Commands.RemoveMember;
using FamilyCare.Application.FamilyManagement.Commands.UpdateMemberDetails;
using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Queries.GetFamilyMembers;
using FamilyCare.Domain.Common;
using FamilyCare.Domain.FamilyManagement;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class MemberEndpoints
{
    public static IEndpointRouteBuilder MapMemberEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/families/{familyId:guid}/members")
            .WithTags("Members")
            .RequireAuthorization();

        // GET /api/v1/families/{familyId}/members
        group.MapGet("/", async (
            Guid familyId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetFamilyMembersQuery(FamilyId.From(familyId)),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetFamilyMembers")
        .WithSummary("Lists all members of the family.")
        .Produces<IReadOnlyList<FamilyMemberDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // DELETE /api/v1/families/{familyId}/members/{memberId}
        group.MapDelete("/{memberId:guid}", async (
            Guid familyId,
            Guid memberId,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new RemoveMemberCommand(
                    FamilyId.From(familyId),
                    FamilyMemberId.From(memberId)),
                ct);
            return Results.NoContent();
        })
        .WithName("RemoveMember")
        .WithSummary("Removes a member from the family. Self-removal is allowed; otherwise must be Owner/Admin.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        // PATCH /api/v1/families/{familyId}/members/{memberId}/role
        group.MapPatch("/{memberId:guid}/role", async (
            Guid familyId,
            Guid memberId,
            [FromBody] ChangeMemberRoleRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new ChangeMemberRoleCommand(
                    FamilyId.From(familyId),
                    FamilyMemberId.From(memberId),
                    request.NewRole),
                ct);
            return Results.NoContent();
        })
        .WithName("ChangeMemberRole")
        .WithSummary("Changes a member's role. Only the Owner can do this.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        // PUT /api/v1/families/{familyId}/members/{memberId}/privacy-rules/{category}
        group.MapPut("/{memberId:guid}/privacy-rules/{category}", async (
            Guid familyId,
            Guid memberId,
            DataCategory category,
            [FromBody] ChangePrivacyRuleRequest request,
            ISender sender,
            CancellationToken ct) =>
        {
            var allowedIds = request.AllowedMemberIds?
                .Select(FamilyMemberId.From)
                .ToList();

            await sender.Send(
                new ChangePrivacyRuleCommand(
                    FamilyId.From(familyId),
                    FamilyMemberId.From(memberId),
                    category,
                    request.NewScope,
                    allowedIds),
                ct);
            return Results.NoContent();
        })
        .WithName("ChangePrivacyRule")
        .WithSummary("Updates the privacy rule for a member's data category.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPatch("/{memberId:guid}/details", async (
            Guid familyId,
            Guid memberId,
            [FromBody] UpdateMemberDetailsRequest body,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(
                new UpdateMemberDetailsCommand(
                    FamilyId.From(familyId),
                    FamilyMemberId.From(memberId),
                    body.DisplayName,
                    body.BirthDate,
                    body.Relationship),
                ct);
            return Results.NoContent();
        })
        .WithName("UpdateMemberDetails")
        .WithSummary("Updates editable details (displayName, birthDate, relationship) of a family member.")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    public sealed record ChangeMemberRoleRequest(Role NewRole);

    public sealed record ChangePrivacyRuleRequest(
        VisibilityScope NewScope,
        IReadOnlyCollection<Guid>? AllowedMemberIds);

    public sealed record UpdateMemberDetailsRequest(
        string? DisplayName,
        DateOnly? BirthDate,
        RelationshipType? Relationship);
}
