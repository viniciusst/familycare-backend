using FamilyCare.Application.Identity.Commands.Login;
using FamilyCare.Application.Identity.Commands.Logout;
using FamilyCare.Application.Identity.Commands.RefreshTokens;
using FamilyCare.Application.Identity.Commands.RegisterUser;
using FamilyCare.Application.Identity.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FamilyCare.Api.Endpoints.V1;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        // POST /api/v1/auth/register
        group.MapPost("/register", async (
            [FromBody] RegisterUserCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var response = await sender.Send(command, ct);
            return Results.Created($"/api/v1/users/{response.UserId}", response);
        })
        .WithName("Register")
        .WithSummary("Registers a new user account.")
        .Produces<RegisterUserResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict)
        .AllowAnonymous()
        .RequireRateLimiting(Setup.RateLimitingSetup.AuthPolicy);

        // POST /api/v1/auth/login
        group.MapPost("/login", async (
            [FromBody] LoginCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var tokens = await sender.Send(command, ct);
            return Results.Ok(tokens);
        })
        .WithName("Login")
        .WithSummary("Authenticates a user and returns access + refresh tokens.")
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .AllowAnonymous()
        .RequireRateLimiting(Setup.RateLimitingSetup.AuthPolicy);

        // POST /api/v1/auth/refresh
        group.MapPost("/refresh", async (
            [FromBody] RefreshTokensCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            var tokens = await sender.Send(command, ct);
            return Results.Ok(tokens);
        })
        .WithName("RefreshTokens")
        .WithSummary("Rotates a refresh token, returning a fresh access + refresh pair.")
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .AllowAnonymous();

        // POST /api/v1/auth/logout
        group.MapPost("/logout", async (
            [FromBody] LogoutCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            await sender.Send(command, ct);
            return Results.NoContent();
        })
        .WithName("Logout")
        .WithSummary("Revokes a refresh token.")
        .Produces(StatusCodes.Status204NoContent)
        .RequireAuthorization();

        // GET /api/v1/auth/me
        group.MapGet("/me", async (ISender sender, CancellationToken ct) =>
        {
            var profile = await sender.Send(new GetMeQuery(), ct);
            return Results.Ok(profile);
        })
        .WithName("GetMe")
        .WithSummary("Returns the authenticated user's profile.")
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .RequireAuthorization();

        return app;
    }
}
