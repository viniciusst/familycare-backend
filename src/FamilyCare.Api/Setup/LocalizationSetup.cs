using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace FamilyCare.Api.Setup;

public static class LocalizationSetup
{
    private static readonly string[] SupportedCultureCodes = ["pt-BR", "en-CA", "fr-CA"];

    public static IServiceCollection AddFamilyCareLocalization(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = SupportedCultureCodes
                .Select(c => new CultureInfo(c))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture("pt-BR", "pt-BR");
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;
            options.ApplyCurrentCultureToResponseHeaders = true;

            // Order: Accept-Language header → query string → cookie
            options.RequestCultureProviders =
            [
                new AcceptLanguageHeaderRequestCultureProvider(),
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider()
            ];
        });

        return services;
    }
}
