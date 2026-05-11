namespace FamilyCare.Application.Common.Abstractions;

/// <summary>
/// Unit of Work abstraction. Implemented by Infrastructure (EF Core SaveChanges).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists all pending changes and returns the number of affected rows.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
