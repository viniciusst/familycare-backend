using System.Text.Json;
using FamilyCare.Application.Common.Exceptions;
using FamilyCare.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using AppValidationException = FamilyCare.Application.Common.Exceptions.ValidationException;

namespace FamilyCare.Api.Middleware;

/// <summary>
/// Converts Domain/Application exceptions into RFC 7807 ProblemDetails responses.
/// Unknown exceptions become 500 with a generic message (details only in logs).
/// </summary>
public sealed partial class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (status, problem) = MapException(exception);

        if (status >= 500)
        {
            LogUnhandled(_logger, exception);
        }
        else
        {
            LogHandled(_logger, exception.GetType().Name, exception.Message);
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        // IMPORTANT: serialize using the runtime type, not the static ProblemDetails type.
        // Otherwise subclass properties (e.g. ValidationProblemDetails.Errors) are dropped.
        var json = JsonSerializer.Serialize(problem, problem.GetType(), JsonOptions);

        await context.Response.WriteAsync(json);
    }

    private (int Status, ProblemDetails Problem) MapException(Exception ex) => ex switch
    {
        AppValidationException v => (
            StatusCodes.Status400BadRequest,
            new ValidationProblemDetails(v.Errors.ToDictionary(e => e.Key, e => e.Value))
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Detail = v.Message
            }),

        // Malformed JSON, missing required field on the body, etc.
        BadHttpRequestException bad => (
            StatusCodes.Status400BadRequest,
            BuildProblem(400, "Bad Request", "bad_request",
                _env.IsDevelopment() ? bad.Message : "The request payload could not be parsed.")),

        UnauthenticatedException u => (
            StatusCodes.Status401Unauthorized,
            BuildProblem(401, "Unauthenticated", u.Code, u.Message)),

        ForbiddenException f => (
            StatusCodes.Status403Forbidden,
            BuildProblem(403, "Forbidden", f.Code, f.Message)),

        NotFoundException nf => (
            StatusCodes.Status404NotFound,
            BuildProblem(404, "Not Found", nf.Code, nf.Message)),

        ConflictException c => (
            StatusCodes.Status409Conflict,
            BuildProblem(409, "Conflict", c.Code, c.Message)),

        BusinessRuleViolationException br => (
            StatusCodes.Status422UnprocessableEntity,
            BuildProblem(422, "Business rule violation", br.Code, br.Message)),

        InvalidEntityStateException inv => (
            StatusCodes.Status400BadRequest,
            BuildProblem(400, "Invalid state", inv.Code, inv.Message)),

        EntityNotFoundException dnf => (
            StatusCodes.Status404NotFound,
            BuildProblem(404, "Not Found", dnf.Code, dnf.Message)),

        DomainException d => (
            StatusCodes.Status400BadRequest,
            BuildProblem(400, "Domain error", d.Code, d.Message)),

        _ => (
            StatusCodes.Status500InternalServerError,
            BuildProblem(
                500,
                "Internal Server Error",
                "internal_error",
                _env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred."))
    };

    private static ProblemDetails BuildProblem(int status, string title, string code, string detail)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = $"https://familycare.local/errors/{code}"
        };
        problem.Extensions["code"] = code;
        return problem;
    }

    [LoggerMessage(EventId = 9000, Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandled(ILogger logger, Exception exception);

    [LoggerMessage(EventId = 9001, Level = LogLevel.Warning, Message = "{ExceptionType}: {ExceptionMessage}")]
    private static partial void LogHandled(ILogger logger, string exceptionType, string exceptionMessage);
}
