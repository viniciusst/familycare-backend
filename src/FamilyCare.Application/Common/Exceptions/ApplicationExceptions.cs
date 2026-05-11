using FluentValidation.Results;

namespace FamilyCare.Application.Common.Exceptions;

/// <summary>Base class for application-layer exceptions.</summary>
public abstract class ApplicationException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Thrown when FluentValidation fails on a command/query.</summary>
public sealed class ValidationException : ApplicationException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("application.validation_failed", "One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : base("application.validation_failed", "One or more validation failures have occurred.")
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}

/// <summary>Thrown when an authenticated user is required but absent.</summary>
public sealed class UnauthenticatedException()
    : ApplicationException("application.unauthenticated", "Authentication is required.");

/// <summary>Thrown when the current user lacks permission to perform the requested action.</summary>
public sealed class ForbiddenException(string message)
    : ApplicationException("application.forbidden", message);

/// <summary>Thrown when an entity referenced by ID cannot be found.</summary>
public sealed class NotFoundException(string entityName, object id)
    : ApplicationException("application.not_found", $"{entityName} with id '{id}' was not found.");

/// <summary>Thrown when an entity violates uniqueness rules.</summary>
public sealed class ConflictException(string code, string message)
    : ApplicationException(code, message);
