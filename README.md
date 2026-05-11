# FamilyCare 🏥

[![CI](https://github.com/USERNAME/REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/USERNAME/REPO/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![PostgreSQL 17](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)

Health and well-being platform for extended families.

## 📋 About

Multi-platform system (.NET 10 backend + Next.js web + iOS SwiftUI) focused on:

- **Medical history** (appointments, exams, vaccines, allergies, chronic conditions)
- **Medications** (reminders, dosages, inventory)
- **Symptoms and daily well-being**
- **Physical activity and habits**
- **Nutrition and hydration**

Supports **multiple families per user**, **per-category configurable privacy**, and **multi-language (pt-BR, en-CA, fr-CA)**.

## 🏗️ Architecture

Clean Architecture + DDD + CQRS (MediatR).

```
FamilyCare/
├── src/
│   ├── FamilyCare.Domain/          → Entities, VOs, events, pure rules
│   ├── FamilyCare.Application/     → Commands, queries, handlers, validators
│   ├── FamilyCare.Infrastructure/  → EF Core, JWT, file storage
│   └── FamilyCare.Api/             → Endpoints, middleware, OpenAPI
└── tests/
    ├── FamilyCare.Domain.Tests/
    ├── FamilyCare.Application.Tests/
    └── FamilyCare.Api.IntegrationTests/
```

### Bounded Contexts

| Context               | Responsibility                                                  |
| --------------------- | --------------------------------------------------------------- |
| **Identity**          | Users, authentication, refresh tokens                           |
| **FamilyManagement**  | Families, members, roles, privacy rules, invitations            |
| **MedicalHistory**    | Appointments, exams, vaccines, allergies, conditions, attachments |

## 🚀 Stack

- **.NET 10** (LTS, supported until Nov/2028)
- **C# 14**
- **EF Core 10** + **PostgreSQL 17**
- **MediatR** (CQRS)
- **FluentValidation**
- **xUnit** + **Moq** + **AutoFixture/AutoMoq** + **FluentAssertions**
- **Docker Compose** (Postgres + pgAdmin + API)
- **Scalar** + **Swagger UI** (OpenAPI exploration)

## 🐳 Spinning up the environment

```bash
# Start Postgres + pgAdmin + API
docker compose up -d

# Follow API logs
docker compose logs -f api

# Tear down (keeps volumes)
docker compose down

# Tear down and wipe volumes (careful: clears the DB)
docker compose down -v
```

### Endpoints

| Service     | URL                              | Credentials                    |
| ----------- | -------------------------------- | ------------------------------ |
| API         | http://localhost:8080            | -                              |
| Health      | http://localhost:8080/health     | -                              |
| Scalar UI   | http://localhost:8080/scalar     | (dev only)                     |
| Swagger UI  | http://localhost:8080/swagger    | (dev only)                     |
| OpenAPI doc | http://localhost:8080/openapi/v1.json | (dev only)                |
| pgAdmin     | http://localhost:5050            | admin@familycare.com / admin   |
| Postgres    | localhost:5432                   | familycare / familycare_dev    |

## 🛠️ Local development (no Docker)

Prerequisites: .NET 10 SDK + a running Postgres instance (or just the `postgres` service from compose).

```bash
# Restore dependencies
dotnet restore

# Run the API
dotnet run --project src/FamilyCare.Api

# Run tests
dotnet test
```

### EF Core migrations

```bash
# Add a new migration
dotnet ef migrations add <Name> \
  --project src/FamilyCare.Infrastructure \
  --startup-project src/FamilyCare.Api \
  --output-dir Persistence/Migrations

# Apply migrations (the API does this automatically at startup with retry/backoff)
dotnet ef database update \
  --project src/FamilyCare.Infrastructure \
  --startup-project src/FamilyCare.Api
```

## 🗺️ Roadmap

- [x] **Phase 1**: FamilyManagement + MedicalHistory + Docker + Auth
- [ ] **Phase 2**: Medications
- [ ] **Phase 3**: Symptoms and daily well-being
- [ ] **Phase 4**: Physical activity and habits
- [ ] **Phase 5**: Nutrition and hydration
- [ ] **Frontend**: Next.js 14
- [ ] **iOS**: SwiftUI + SwiftData

## 📝 Notes

- EF Core migrations run automatically at startup with exponential backoff.
- JWT access tokens + refresh tokens with rotation and reuse detection.
- File storage abstracted (Local in dev, S3-compatible later).
- `Directory.Build.props` centralizes nullable + warnings-as-errors + analyzer config.
- All code, comments and documentation are written in English.

## 🔒 Security

The `appsettings.json` shipped in the repo contains **development-only** values. Before deploying to any non-local environment:

1. **Replace the JWT signing key.** The default in `appsettings.json` is a placeholder. Generate a strong key (minimum 32 chars) and set it via:
   - User Secrets: `dotnet user-secrets set "Jwt:Key" "<your-key>" --project src/FamilyCare.Api`
   - Environment variable: `Jwt__Key=<your-key>`
2. **Replace the Postgres connection string.** Same approach — User Secrets or `ConnectionStrings__Postgres`.
3. **Set `ASPNETCORE_ENVIRONMENT=Production`.** This disables Scalar/Swagger UIs and stack traces in error responses.
4. **Configure CORS** with the actual origins instead of `localhost:3000`.

## 📄 License

[MIT](LICENSE) © 2026 Vinicius
