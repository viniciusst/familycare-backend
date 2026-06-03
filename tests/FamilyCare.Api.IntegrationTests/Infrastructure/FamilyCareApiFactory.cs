using FamilyCare.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Respawn;
using System.Text;
using System.Threading.RateLimiting;
using Testcontainers.PostgreSql;

namespace FamilyCare.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the API in-process against an ephemeral PostgreSQL container.
///
/// One container is shared by all tests in this collection (xUnit's
/// <see cref="IAsyncLifetime"/> on the fixture). Between tests we reset the
/// data using Respawn, keeping the schema and migrations intact.
///
/// Configuration overrides:
///  - ConnectionStrings:Postgres points to the container
///  - Jwt:Key is a deterministic key valid for tests (at least 32 chars)
///  - Disables the production DatabaseInitializer (we migrate manually)
///  - Replaces the auth rate-limiter policy with a permissive one
///  - Re-configures JwtBearer validation with the test key, because
///    JwtBearerOptions captures the IssuerSigningKey in a closure
///    during AddJwtBearer(...) — same trap as the DbContext.
/// </summary>
public sealed class FamilyCareApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string TestJwtKey =
        "INTEGRATION_TEST_KEY_NEVER_USE_IN_PRODUCTION_32+CHARS";
    private const string TestIssuer = "FamilyCare.Api";
    private const string TestAudience = "FamilyCare.Clients";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("familycare_tests")
        .WithUsername("familycare")
        .WithPassword("familycare_test")
        .Build();

    private Respawner? _respawner;
    private NpgsqlConnection? _respawnerConnection;

    public string ConnectionString => _postgres.GetConnectionString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
                ["Jwt:Key"] = TestJwtKey,
                ["Jwt:Issuer"] = TestIssuer,
                ["Jwt:Audience"] = TestAudience,
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "30",
                ["Localization:DefaultCulture"] = "en-CA",
                ["Localization:SupportedCultures:0"] = "pt-BR",
                ["Localization:SupportedCultures:1"] = "en-CA",
                ["Localization:SupportedCultures:2"] = "fr-CA",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove production DatabaseInitializer. We migrate manually in
            // InitializeAsync below; the hosted service would otherwise retry
            // for 7.5 minutes against a stale connection string.
            var initializerDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IHostedService) &&
                d.ImplementationType?.Name == "DatabaseInitializer");

            if (initializerDescriptor is not null)
            {
                services.Remove(initializerDescriptor);
            }

            // Re-register DbContext with the container connection string.
            var dbContextDescriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<FamilyCareDbContext>));

            if (dbContextDescriptor is not null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<FamilyCareDbContext>(options =>
            {
                options.UseNpgsql(_postgres.GetConnectionString(), npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(FamilyCareDbContext).Assembly.FullName);
                });
                options.UseSnakeCaseNamingConvention();
            });

            // Wipe and re-install rate limiter with permissive policy.
            services.RemoveAll<IConfigureOptions<RateLimiterOptions>>();
            services.RemoveAll<IPostConfigureOptions<RateLimiterOptions>>();

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddPolicy("auth", context =>
                {
                    var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: key,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10_000,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        });
                });
            });

            // Re-configure JwtBearer validation with the TEST key. The
            // production AddJwtBearer(...) captured the original
            // IssuerSigningKey in a closure before our config overrides
            // took effect, so it would otherwise reject our test-signed
            // tokens with 401. PostConfigure runs last, after every Configure.
            services.PostConfigure<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = TestIssuer,
                        ValidateAudience = true,
                        ValidAudience = TestAudience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(TestJwtKey)),
                        ClockSkew = TimeSpan.Zero,
                    };
                });
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // Apply EF Core migrations against the fresh container.
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FamilyCareDbContext>();
        await db.Database.MigrateAsync();

        // Build a Respawner that targets the public schema.
        _respawnerConnection = new NpgsqlConnection(_postgres.GetConnectionString());
        await _respawnerConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_respawnerConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = [new Respawn.Graph.Table("__EFMigrationsHistory")],
        });
    }

    /// <summary>
    /// Deletes all rows from every table (in dependency order), keeping the
    /// schema. Call before each test that needs a clean slate.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null || _respawnerConnection is null)
        {
            throw new InvalidOperationException(
                "Factory not initialized. xUnit must call InitializeAsync first.");
        }

        await _respawner.ResetAsync(_respawnerConnection);
    }

    public new async Task DisposeAsync()
    {
        if (_respawnerConnection is not null)
        {
            await _respawnerConnection.DisposeAsync();
        }
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}