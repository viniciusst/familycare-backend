namespace FamilyCare.Api.Setup;

public static class CorsSetup
{
    public const string DefaultPolicy = "DefaultCorsPolicy";

    public static IServiceCollection AddFamilyCareCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowed = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                      ?? ["http://localhost:3000", "https://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(DefaultPolicy, builder =>
            {
                builder
                    .WithOrigins(allowed)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
