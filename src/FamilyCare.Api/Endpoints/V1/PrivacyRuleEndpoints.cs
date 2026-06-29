using FamilyCare.Application.FamilyManagement.Dtos;
using FamilyCare.Application.FamilyManagement.Queries.GetPrivacyRulesByMember;
using FamilyCare.Domain.Common;
using MediatR;

namespace FamilyCare.Api.Endpoints.V1;

/// <summary>
/// Privacy rule endpoints. The PUT to change a rule lives in
/// MemberEndpoints (alongside other member-scoped mutations); this file
/// only adds the read-side query for listing a member's configured rules.
/// </summary>
public static class PrivacyRuleEndpoints
{
    public static IEndpointRouteBuilder MapPrivacyRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var scoped = app.MapGroup("/api/v1/families/{familyId:guid}/members/{memberId:guid}/privacy-rules")
            .WithTags("Privacy Rules")
            .RequireAuthorization();

        // GET — list all privacy rules configured for a member.
        scoped.MapGet("/", async (
            Guid familyId,
            Guid memberId,
            ISender sender,
            CancellationToken ct) =>
        {
            var result = await sender.Send(
                new GetPrivacyRulesByMemberQuery(
                    FamilyId.From(familyId),
                    FamilyMemberId.From(memberId)),
                ct);
            return Results.Ok(result);
        })
        .WithName("GetPrivacyRulesByMember")
        .WithSummary("Lists privacy rules configured for a family member.")
        .Produces<IReadOnlyList<PrivacyRuleDto>>()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}