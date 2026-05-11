using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace FamilyCare.Infrastructure.Persistence.Initialization;

/// <summary>
/// Runs <c>DbContext.Database.MigrateAsync</c> on application startup.
/// Uses exponential backoff to wait for Postgres to become ready, since in
/// Docker the API container may start before the DB is fully accepting connections.
/// </summary>
public sealed partial class DatabaseInitializer(
    IServiceProvider serviceProvider,
    ILogger<DatabaseInitializer> logger)
    : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<DatabaseInitializer> _logger = logger;

    private const int MaxAttempts = 15;
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxDelay = TimeSpan.FromSeconds(30);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<FamilyCareDbContext>();

        var delay = InitialDelay;

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                LogAttempting(_logger, attempt, MaxAttempts);
                await dbContext.Database.MigrateAsync(cancellationToken);
                LogSuccess(_logger);
                return;
            }
            catch (NpgsqlException ex) when (attempt < MaxAttempts)
            {
                LogTransientFailure(_logger, ex, attempt, delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
                // Exponential backoff capped at MaxDelay
                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, MaxDelay.TotalSeconds));
            }
            catch (Exception ex)
            {
                LogFatal(_logger, ex);
                throw;
            }
        }

        throw new InvalidOperationException(
            $"Could not connect to the database after {MaxAttempts} attempts.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(EventId = 5000, Level = LogLevel.Information,
        Message = "Database migration attempt {Attempt}/{Max}")]
    private static partial void LogAttempting(ILogger logger, int attempt, int max);

    [LoggerMessage(EventId = 5001, Level = LogLevel.Information,
        Message = "Database migrations applied successfully.")]
    private static partial void LogSuccess(ILogger logger);

    [LoggerMessage(EventId = 5002, Level = LogLevel.Warning,
        Message = "Database not ready (attempt {Attempt}); retrying in {DelaySeconds}s.")]
    private static partial void LogTransientFailure(ILogger logger, Exception exception, int attempt, double delaySeconds);

    [LoggerMessage(EventId = 5003, Level = LogLevel.Critical,
        Message = "Fatal error initializing the database.")]
    private static partial void LogFatal(ILogger logger, Exception exception);
}
