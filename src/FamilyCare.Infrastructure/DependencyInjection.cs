using FamilyCare.Application.Common.Abstractions;
using FamilyCare.Application.FamilyManagement.Abstractions;
using FamilyCare.Application.FamilyManagement.Repositories;
using FamilyCare.Application.Identity.Abstractions;
using FamilyCare.Application.Identity.Repositories;
using FamilyCare.Application.MedicalHistory.Abstractions;
using FamilyCare.Application.MedicalHistory.Repositories;
using FamilyCare.Infrastructure.Identity.Authentication;
using FamilyCare.Infrastructure.Identity.Services;
using FamilyCare.Infrastructure.Persistence;
using FamilyCare.Infrastructure.Persistence.DomainEvents;
using FamilyCare.Infrastructure.Persistence.Initialization;
using FamilyCare.Infrastructure.Persistence.Repositories.FamilyManagement;
using FamilyCare.Infrastructure.Persistence.Repositories.Identity;
using FamilyCare.Infrastructure.Persistence.Repositories.MedicalHistory;
using FamilyCare.Infrastructure.Storage;
using FamilyCare.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FamilyCare.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // EF Core + PostgreSQL + snake_case naming
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("ConnectionStrings:Postgres is not configured.");

        services.AddDbContext<FamilyCareDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(FamilyCareDbContext).Assembly.FullName);
                npgsql.EnableRetryOnFailure(maxRetryCount: 5);
            });
            options.UseSnakeCaseNamingConvention();
        });

        // Unit of Work + Domain Events
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

        // Identity
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32,
                      "Jwt:Key must be at least 32 characters long.")
            .ValidateOnStart();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthTokenService, AuthTokenService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // FamilyManagement
        services.AddScoped<IFamilyRepository, FamilyRepository>();
        services.AddScoped<IMembershipResolver, MembershipResolver>();

        // MedicalHistory
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IVaccineRepository, VaccineRepository>();
        services.AddScoped<IAllergyRepository, AllergyRepository>();
        services.AddScoped<IChronicConditionRepository, ChronicConditionRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        // Storage
        services.AddOptions<StorageOptions>()
            .Bind(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();

        // Cross-cutting
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Hosted services
        services.AddHostedService<DatabaseInitializer>();

        return services;
    }
}
