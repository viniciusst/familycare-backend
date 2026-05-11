using Microsoft.OpenApi;

namespace FamilyCare.Api.Setup;

public static class OpenApiSetup
{
    public static IServiceCollection AddFamilyCareOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi("v1", options =>
        {
            options.AddDocumentTransformer((document, _, _) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "FamilyCare API",
                    Version = "v1",
                    Description = "Health and well-being management platform for extended families.",
                    Contact = new OpenApiContact { Name = "FamilyCare", Email = "contact@familycare.com" }
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Paste a JWT access token (without the 'Bearer ' prefix)."
                };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}
