using FamilyCare.Domain.Common;

namespace FamilyCare.Application.Common.Abstractions;

/// <summary>
/// Provides information about the currently authenticated user. Implemented
/// by the API layer reading from the JWT/HttpContext.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>True if a user is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>The authenticated user's id, or null if not authenticated.</summary>
    UserId? UserId { get; }

    /// <summary>Throws if no user is authenticated; otherwise returns the id.</summary>
    UserId RequireUserId();
}
