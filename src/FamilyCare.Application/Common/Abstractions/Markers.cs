using MediatR;

namespace FamilyCare.Application.Common.Abstractions;

/// <summary>
/// Marker interface for write operations. Commands implementing this run inside
/// a unit-of-work scope (SaveChanges + DispatchDomainEvents). Queries should
/// NOT implement this — they should be IRequest&lt;T&gt; only.
/// </summary>
public interface ICommand : IRequest;

/// <summary>
/// Marker interface for write operations that return a value.
/// </summary>
public interface ICommand<TResponse> : IRequest<TResponse>;

/// <summary>
/// Marker interface for read operations.
/// </summary>
public interface IQuery<TResponse> : IRequest<TResponse>;
