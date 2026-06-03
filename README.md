# FamilyCare API 🏥

[![CI](https://github.com/viniciusst/familycare-backend/actions/workflows/ci.yml/badge.svg?branch=develop)](https://github.com/viniciusst/familycare-backend/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet)](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
[![PostgreSQL 17](https://img.shields.io/badge/PostgreSQL-17-336791?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Tests](https://img.shields.io/badge/tests-231%20passing-brightgreen)](#testing)
[![Architecture](https://img.shields.io/badge/architecture-Clean%20%2B%20DDD%20%2B%20CQRS-blue)](docs/ARCHITECTURE.md)

> Production-grade backend for a multi-platform family health and well-being platform.
> Built with **Clean Architecture + DDD + CQRS** on **.NET 10** and **PostgreSQL 17**.

---

## ✨ Highlights

- 🏛️ **Clean Architecture** with strict layer boundaries enforced at the project-reference level
- 🎯 **Domain-Driven Design** with rich aggregates (Family, User, Appointment, ...) and strongly-typed IDs
- ⚡ **CQRS** via MediatR with three pipeline behaviors (Validation → UnitOfWork → Logging)
- 🔐 **JWT + Refresh Token Rotation** with reuse detection
- 🛡️ **Per-category Privacy Engine** — users control which family members can see each data category
- 🌍 **Multi-language API** (pt-BR / en-CA / fr-CA) with RFC 7807 ProblemDetails
- 🧪 **231 automated tests** covering Domain, Application, and full-stack API integration
- 🐳 **Docker-first** local development with `docker compose up`
- 📜 **OpenAPI** (Scalar UI) auto-generated for all 47 endpoints

---

## 🎬 Quick Start

```bash
# 1. Clone
git clone https://github.com/viniciusst/familycare-backend.git
cd familycare-backend

# 2. Start the database, API, and Scalar docs
docker compose up -d

# 3. Open the API docs
open http://localhost:8080/scalar/v1
```

That's it. The API auto-migrates the database on first start.

> **Without Docker?** See [Running locally](#running-locally-without-docker).

---

## 🏗️ Architecture at a glance

```mermaid
graph TB
    subgraph "Clients"
        WEB[Next.js Web]
        IOS[iOS SwiftUI]
    end

    subgraph "FamilyCare.Api"
        EP[Minimal API Endpoints]
        MW[Middleware<br/>Auth · Localization · ProblemDetails · RateLimit]
    end

    subgraph "FamilyCare.Application"
        MED[MediatR Pipeline]
        BEH[Behaviors<br/>Validation → UnitOfWork → Logging]
        CMD[Commands & Queries]
        VAL[FluentValidation Validators]
        GUARD[MedicalAccessGuard<br/>Privacy Policy Evaluator]
    end

    subgraph "FamilyCare.Domain"
        AGG[Aggregates<br/>Family · User · Appointment · ...]
        VO[Value Objects<br/>Email · PasswordHash · Role · ...]
        EVT[Domain Events]
    end

    subgraph "FamilyCare.Infrastructure"
        EF[EF Core 10 + Npgsql]
        REPO[Repositories]
        JWT[JwtTokenService]
        HASH[BCrypt PasswordHasher]
    end

    DB[(PostgreSQL 17<br/>snake_case)]

    WEB --> EP
    IOS --> EP
    EP --> MW
    MW --> MED
    MED --> BEH
    BEH --> CMD
    CMD --> VAL
    CMD --> GUARD
    CMD --> AGG
    AGG --> VO
    AGG --> EVT
    CMD --> REPO
    REPO --> EF
    EF --> DB

    classDef api fill:#512BD4,color:#fff
    classDef app fill:#1e88e5,color:#fff
    classDef domain fill:#388e3c,color:#fff
    classDef infra fill:#f57c00,color:#fff
    classDef client fill:#9e9e9e,color:#fff

    class EP,MW api
    class MED,BEH,CMD,VAL,GUARD app
    class AGG,VO,EVT domain
    class EF,REPO,JWT,HASH infra
    class WEB,IOS client
```

**Dependency rule:** `Api → Application → Domain ← Infrastructure → Domain`.
Domain has zero external dependencies. Application depends only on Domain.

For a deeper dive, read [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

---

## 🧰 Tech stack

| Layer | Choice | Why |
|---|---|---|
| **Runtime** | .NET 10 (LTS) | Modern C# features, performance, long-term support |
| **API style** | Minimal APIs grouped by feature | Less ceremony than MVC, easy to test |
| **CQRS** | MediatR 12 | Battle-tested pipeline; behaviors separate cross-cutting concerns |
| **Validation** | FluentValidation 11 | Composable validators, automatic via pipeline behavior |
| **Persistence** | EF Core 10 + Npgsql | snake_case via `EFCore.NamingConventions` matches Postgres idioms |
| **Database** | PostgreSQL 17 | Robust ACID, JSONB, full-text, mature ecosystem |
| **Auth** | JWT + refresh tokens | Stateless, rotation with reuse-detection |
| **Password hashing** | BCrypt | Adaptive cost, industry standard |
| **API docs** | OpenAPI + Scalar | Auto-generated, beautiful UI, no manual maintenance |
| **Testing** | xUnit + Moq + Testcontainers + Respawn | Unit + integration with **real Postgres** in containers |
| **CI/CD** | GitHub Actions | Build, test, Docker build on every PR |
| **Dependency management** | Central Package Management (CPM) | Single source of truth for versions across projects |

---

## 🗺️ API surface

47 endpoints across 6 feature areas, all under `/api/v1/`:

| Area | Endpoints | Key flows |
|---|---|---|
| **Auth** | 5 | Register · Login · Refresh · Logout · Me |
| **Families** | 7 | Create · Rename · Transfer ownership · List · Get · Members · Remove |
| **Invitations** | 4 | Invite · Accept · Decline · Revoke |
| **Members** | 3 | Change role · Change privacy rule · List |
| **Medical history** | 24 | Appointments · Exams · Vaccines · Allergies · Chronic conditions · Attachments |
| **Health** | 2 | `/health` · `/health/ready` |

Browse the full surface in Scalar at `http://localhost:8080/scalar/v1` after running `docker compose up`.

---

## 🔐 Authentication flow

```mermaid
sequenceDiagram
    autonumber
    participant C as Client
    participant API as FamilyCare API
    participant DB as PostgreSQL

    C->>API: POST /auth/register {email, password}
    API->>DB: INSERT user (password = BCrypt hash)
    API-->>C: 201 Created {userId}

    C->>API: POST /auth/login {email, password}
    API->>DB: Verify hash<br/>INSERT refresh_token
    API-->>C: 200 OK {accessToken, refreshToken, expiresAt}

    Note over C,API: Use accessToken in Authorization: Bearer <token>

    C->>API: GET /auth/me [Bearer]
    API-->>C: 200 OK {profile}

    Note over C,API: Access token expires after 60 min

    C->>API: POST /auth/refresh {refreshToken}
    API->>DB: Find token<br/>Mark replaced_by<br/>INSERT new token
    API-->>C: 200 OK {newAccessToken, newRefreshToken}

    Note over C,API: Reusing the OLD token now fails

    C->>API: POST /auth/refresh {OLD refreshToken}
    API->>DB: Find token (already replaced)
    API-->>C: 403 Forbidden
```

**Refresh-token rotation with reuse detection** is the production-recommended pattern: a stolen refresh token becomes invalid the moment the legitimate client rotates it.

---

## 🛡️ Privacy model

Each family member can set, **per data category**, who is allowed to see their information:

```mermaid
graph LR
    M[Member<br/>'Vinicius'] -->|MedicalHistory| P1[Private]
    M -->|Medications| P2[FamilyAdmins only]
    M -->|Wellbeing| P3[AllFamily]
    M -->|Activity| P4[Custom:<br/>spouse + sibling]
    M -->|Nutrition| P5[AllFamily]
```

Categories: `MedicalHistory`, `Medications`, `Wellbeing`, `Activity`, `Nutrition`.
Scopes: `Private`, `FamilyAdmins`, `AllFamily`, `Custom`.

Every read/write of medical data goes through `MedicalAccessGuard`, which combines the requester's role, the target member's privacy rules, and the data category to authorize or reject.

---

## 🧪 Testing

| Layer | Project | Tests | What it covers |
|---|---|---|---|
| **Domain** | `FamilyCare.Domain.Tests` | 84 | Entity invariants, value object validation, state transitions, domain events |
| **Application** | `FamilyCare.Application.Tests` | 133 | Handlers, validators, behaviors (Validation/UnitOfWork/Logging) with mocked infrastructure |
| **API integration** | `FamilyCare.Api.IntegrationTests` | 14 | Full HTTP round-trips against **real PostgreSQL** via Testcontainers + Respawn |
| **Total** | | **231 ✅** | |

Run them all:

```bash
dotnet test
```

Integration tests require Docker to be running (Testcontainers spawns a Postgres container per test run).

---

## 📂 Repository structure

```text
familycare-backend/
├── src/
│   ├── FamilyCare.Domain/             ← Entities, value objects, domain events
│   │   ├── Common/                    ← StronglyTypedIds, base types
│   │   ├── Identity/                  ← User, RefreshToken, Email, PasswordHash
│   │   ├── FamilyManagement/          ← Family, FamilyMember, Invitation, PrivacyRule
│   │   └── MedicalHistory/            ← Appointment, Exam, Vaccine, Allergy, ...
│   ├── FamilyCare.Application/        ← Commands, queries, handlers, validators
│   │   ├── Common/Behaviors/          ← Validation, UnitOfWork, Logging
│   │   ├── Identity/                  ← Register, Login, Refresh, ChangePassword
│   │   ├── FamilyManagement/          ← Family lifecycle handlers
│   │   └── MedicalHistory/            ← Medical handlers + Privacy authorization
│   ├── FamilyCare.Infrastructure/     ← EF Core, repositories, JWT, hashing
│   │   ├── Persistence/               ← DbContext, Migrations, Configurations
│   │   ├── Identity/Services/         ← AuthTokenService, PasswordHasher
│   │   └── Storage/                   ← Attachment storage (local FS)
│   └── FamilyCare.Api/                ← Minimal API endpoints, middleware
│       ├── Endpoints/V1/              ← 10 endpoint groups
│       ├── Middleware/                ← Exception handling, correlation ID
│       └── Setup/                     ← DI composition, auth, rate limiting, localization
├── tests/                             ← 231 tests across 3 layers
├── docs/
│   └── ARCHITECTURE.md                ← Deep dive into architectural decisions
├── docker-compose.yml                 ← Postgres + API + Scalar
├── Directory.Packages.props           ← Central Package Management
└── Directory.Build.props              ← Shared MSBuild config (analyzers, nullable, etc.)
```

---

## 🏃 Running locally without Docker

You need .NET 10 SDK and a PostgreSQL 17 instance.

```bash
# 1. Start a Postgres locally (or use your own)
docker run -d --name familycare-pg \
  -e POSTGRES_USER=familycare \
  -e POSTGRES_PASSWORD=familycare \
  -e POSTGRES_DB=familycare \
  -p 5432:5432 \
  postgres:17-alpine

# 2. Set connection string
export ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=familycare;Username=familycare;Password=familycare"
export Jwt__Key="your-32-plus-character-secret-key-here"

# 3. Run the API (auto-migrates on startup)
dotnet run --project src/FamilyCare.Api
```

API now at `http://localhost:5000`, Scalar at `http://localhost:5000/scalar/v1`.

---

## 🚧 Roadmap

- [x] Phase 1A–1E — Backend (47 endpoints, JWT, CQRS, privacy engine)
- [x] Phase 1F — Test suite (Domain · Application · API integration)
- [x] Phase 1H — Documentation (you are here)
- [ ] Phase 2 — Next.js web frontend
- [ ] Phase 3 — iOS SwiftUI mobile app
- [ ] Cloud deployment

---

## 📄 License

MIT — see [LICENSE](LICENSE).

---

## 👤 Author

**Vinicius Silva Teixeira** — Principal Software Architect
[LinkedIn](https://linkedin.com/in/vinicius-silva-teixeira-09000032) · [GitHub](https://github.com/viniciusst)
