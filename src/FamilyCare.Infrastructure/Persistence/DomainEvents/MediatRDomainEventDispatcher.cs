using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Domain.Common;
using MediatR;

namespace FamilyCare.Infrastructure.Persistence.DomainEvents;

/// <summary>
/// Collects domain events from aggregate roots tracked by the DbContext,
/// clears them, and publishes each as a MediatR INotification.
/// </summary>
public sealed class MediatRDomainEventDispatcher(
    FamilyCareDbContext dbContext,
    IPublisher publisher)
    : IDomainEventDispatcher
{
    private readonly FamilyCareDbContext _dbContext = dbContext;
    private readonly IPublisher _publisher = publisher;

    public async Task DispatchAndClearEventsAsync(CancellationToken cancellationToken = default)
    {
        var aggregatesWithEvents = _dbContext.ChangeTracker
            .Entries()
            .Where(e => e.Entity is IHasDomainEvents { DomainEvents.Count: > 0 })
            .Select(e => (IHasDomainEvents)e.Entity)
            .ToList();

        // Snapshot events and clear before publishing (avoids reentrancy issues)
        var events = aggregatesWithEvents
            .SelectMany(a => a.DomainEvents)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (var domainEvent in events)
        {
            await _publisher.Publish(new DomainEventNotification(domainEvent), cancellationToken);
        }
    }
}

/// <summary>
/// MediatR notification wrapper around a domain event. Handlers in any layer
/// can subscribe via <c>INotificationHandler&lt;DomainEventNotification&gt;</c>.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
