namespace FamilyCare.Domain.Common;

/// <summary>Base class for all domain-related exceptions.</summary>
public abstract class DomainException(string code, string message, Exception? innerException = null)
    : Exception(message, innerException)
{
    /// <summary>Stable code that identifies the kind of error (used by API to map status codes).</summary>
    public string Code { get; } = code;
}

/// <summary>Thrown when an entity invariant is violated (e.g. invalid constructor arguments).</summary>
public sealed class InvalidEntityStateException(string code, string message)
    : DomainException(code, message)
{
    public InvalidEntityStateException(string message)
        : this("domain.invalid_state", message)
    {
    }
}

/// <summary>Thrown when a business rule is violated during an operation.</summary>
public sealed class BusinessRuleViolationException(string code, string message)
    : DomainException(code, message);

/// <summary>Thrown when an entity referenced by ID cannot be found in its aggregate.</summary>
public sealed class EntityNotFoundException(string entityName, object id)
    : DomainException("domain.not_found", $"{entityName} with id '{id}' was not found.");
