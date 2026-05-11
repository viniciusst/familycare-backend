using System.Reflection;
using FamilyCare.Application.Authorization;
using FamilyCare.Application.Common.Behaviors;
using FamilyCare.Application.MedicalHistory.Authorization;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyCare.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR (handlers + notifications)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Order matters: Logging → Validation → UnitOfWork → Handler
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
        });

        // FluentValidation (auto-discovers all validators in the assembly)
        services.AddValidatorsFromAssembly(assembly);

        // Authorization
        services.AddScoped<IPrivacyPolicyEvaluator, PrivacyPolicyEvaluator>();
        services.AddScoped<MedicalAccessGuard>();

        return services;
    }
}
