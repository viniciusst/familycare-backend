using FamilyCare.Application.Common.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyCare.Application.Common.Behaviors;

/// <summary>
/// Pipeline behavior that wraps Command handlers in a unit-of-work scope:
/// after the handler succeeds, SaveChanges is called and Domain Events
/// from tracked aggregates are dispatched via MediatR notifications.
///
/// Only runs for ICommand / ICommand&lt;T&gt;; queries pass through untouched.
/// </summary>
public sealed partial class UnitOfWorkBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    IDomainEventDispatcher domainEventDispatcher,
    ILogger<UnitOfWorkBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher = domainEventDispatcher;
    private readonly ILogger<UnitOfWorkBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Only commit/dispatch events for write operations.
        if (request is not ICommand && request is not ICommand<TResponse>)
        {
            return await next();
        }

        var response = await next();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Dispatch domain events AFTER persistence so handlers can observe
        // a consistent state (e.g. send email referencing the new entity).
        await _domainEventDispatcher.DispatchAndClearEventsAsync(cancellationToken);

        LogCommitted(_logger, typeof(TRequest).Name);

        return response;
    }

    [LoggerMessage(EventId = 2000, Level = LogLevel.Debug, Message = "UnitOfWork committed for {RequestName}")]
    private static partial void LogCommitted(ILogger logger, string requestName);
}
