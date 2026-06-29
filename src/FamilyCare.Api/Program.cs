using FamilyCare.Api.Endpoints.V1;
using FamilyCare.Api.Middleware;
using FamilyCare.Api.Setup;
using FamilyCare.Application;
using FamilyCare.Infrastructure;
using FamilyCare.Infrastructure.Persistence;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API setup
builder.Services.AddFamilyCareAuthentication(builder.Configuration);
builder.Services.AddFamilyCareCors(builder.Configuration);
builder.Services.AddFamilyCareLocalization();
builder.Services.AddFamilyCareOpenApi();
builder.Services.AddFamilyCareRateLimiting();

builder.Services.AddProblemDetails();

// Health checks (also probes the DB)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<FamilyCareDbContext>(name: "postgres", tags: ["ready"]);

var app = builder.Build();

// Pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRequestLocalization();
app.UseCors(CorsSetup.DefaultPolicy);

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

// OpenAPI document
app.MapOpenApi();

if (app.Environment.IsDevelopment())
{
    // Scalar UI at /scalar
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("FamilyCare API")
               .WithTheme(ScalarTheme.BluePlanet)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    // Swagger UI at /swagger
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FamilyCare API v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "FamilyCare API — Swagger";
    });
}

// Health endpoints
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
   .WithTags("Health")
   .WithName("HealthCheck")
   .AllowAnonymous();

app.MapHealthChecks("/health/ready")
   .AllowAnonymous();

// Auth
app.MapAuthEndpoints();

// FamilyManagement
app.MapFamilyEndpoints();
app.MapInvitationEndpoints();
app.MapMemberEndpoints();
app.MapPrivacyRuleEndpoints();

// MedicalHistory
app.MapAppointmentEndpoints();
app.MapExamEndpoints();
app.MapVaccineEndpoints();
app.MapAllergyEndpoints();
app.MapChronicConditionEndpoints();
app.MapAttachmentEndpoints();

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program;
