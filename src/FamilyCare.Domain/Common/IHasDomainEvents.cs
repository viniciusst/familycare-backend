namespace FamilyCare.Domain.Common;

/// <summary>
/// Implemented by aggregate roots to expose accumulated domain events.
/// Allows infrastructure to find them in the change tracker without
/// reflecting over the generic <see cref="AggregateRoot{TId}"/>.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
}
