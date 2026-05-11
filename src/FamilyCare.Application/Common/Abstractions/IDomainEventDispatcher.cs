namespace FamilyCare.Application.Common.Abstractions;

/// <summary>
/// Dispatches domain events accumulated on aggregate roots tracked
/// by the persistence context. The actual implementation lives in Infrastructure
/// (it has access to the DbContext's ChangeTracker).
/// </summary>
public interface IDomainEventDispatcher
{
    /// <summary>
    /// Collects all domain events from tracked aggregates, clears them,
    /// and dispatches each as a MediatR INotification.
    /// </summary>
    Task DispatchAndClearEventsAsync(CancellationToken cancellationToken = default);
}
