using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace FamilyCare.Application.Common.Behaviors;

/// <summary>
/// MediatR pipeline behavior that logs request execution and timing.
/// Uses source-generated LoggerMessage delegates (high-performance logging).
/// </summary>
public sealed partial class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        LogHandling(_logger, requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();
            LogHandled(_logger, requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogFailed(_logger, ex, requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Handling {RequestName}")]
    private static partial void LogHandling(ILogger logger, string requestName);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Handled {RequestName} in {ElapsedMs}ms")]
    private static partial void LogHandled(ILogger logger, string requestName, long elapsedMs);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Error, Message = "Failed handling {RequestName} after {ElapsedMs}ms")]
    private static partial void LogFailed(ILogger logger, Exception exception, string requestName, long elapsedMs);
}
