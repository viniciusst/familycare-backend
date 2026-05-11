namespace FamilyCare.Domain.Common;

/// <summary>
/// Marker interface for all domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the event was raised (UTC).</summary>
    DateTime OccurredOn { get; }
}
