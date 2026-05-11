using System.Threading.RateLimiting;

namespace FamilyCare.Api.Setup;

public static class RateLimitingSetup
{
    public const string AuthPolicy = "auth";

    public static IServiceCollection AddFamilyCareRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Sensitive auth endpoints: 5 requests per minute, per IP.
            options.AddPolicy(AuthPolicy, context =>
            {
                var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });
        });

        return services;
    }
}
