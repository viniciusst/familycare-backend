# ============================================================
# FamilyCare.Api - Multi-stage Dockerfile (.NET 10)
# ============================================================

# --- Stage 1: Build & Restore --------------------------------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj, sln, and Directory.*.props first to leverage Docker layer cache
# for the restore step (only sources changed? Skip restore on rebuild).
#
# Directory.Packages.props is REQUIRED here because we use Central Package
# Management — without it, dotnet restore fails with NU1015 ("PackageReference
# items do not have a version specified").
# Directory.Build.props is needed for the analyzer/nullable/warnings settings
# that may affect restore behavior.
COPY ["FamilyCare.sln", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["src/FamilyCare.Domain/FamilyCare.Domain.csproj", "src/FamilyCare.Domain/"]
COPY ["src/FamilyCare.Application/FamilyCare.Application.csproj", "src/FamilyCare.Application/"]
COPY ["src/FamilyCare.Infrastructure/FamilyCare.Infrastructure.csproj", "src/FamilyCare.Infrastructure/"]
COPY ["src/FamilyCare.Api/FamilyCare.Api.csproj", "src/FamilyCare.Api/"]
COPY ["tests/FamilyCare.Domain.Tests/FamilyCare.Domain.Tests.csproj", "tests/FamilyCare.Domain.Tests/"]
COPY ["tests/FamilyCare.Application.Tests/FamilyCare.Application.Tests.csproj", "tests/FamilyCare.Application.Tests/"]
COPY ["tests/FamilyCare.Api.IntegrationTests/FamilyCare.Api.IntegrationTests.csproj", "tests/FamilyCare.Api.IntegrationTests/"]

# Restore only the API project (transitive references are pulled in)
RUN dotnet restore "src/FamilyCare.Api/FamilyCare.Api.csproj"

# Now copy the rest of the source code and publish
COPY . .
WORKDIR "/src/src/FamilyCare.Api"
RUN dotnet publish "FamilyCare.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# --- Stage 2: Runtime ----------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create a non-root user (security best practice)
RUN groupadd -r familycare && useradd -r -g familycare familycare \
    && mkdir -p /app/storage \
    && chown -R familycare:familycare /app

# Locale packages for runtime culture support (pt-BR, en-CA, fr-CA)
RUN apt-get update \
    && apt-get install -y --no-install-recommends locales \
    && sed -i '/pt_BR.UTF-8/s/^# //g' /etc/locale.gen \
    && sed -i '/en_CA.UTF-8/s/^# //g' /etc/locale.gen \
    && sed -i '/fr_CA.UTF-8/s/^# //g' /etc/locale.gen \
    && locale-gen \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    DOTNET_USE_POLLING_FILE_WATCHER=false

USER familycare

COPY --from=build --chown=familycare:familycare /app/publish .

EXPOSE 8080

# Native Docker healthcheck
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "FamilyCare.Api.dll"]