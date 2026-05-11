# Contributing to FamilyCare

Thanks for your interest in contributing! This guide describes the development workflow and conventions used in this repository.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Postgres + pgAdmin)
- A code editor: Visual Studio 2022+, JetBrains Rider, or VS Code with the C# Dev Kit extension

## Getting started

```bash
# Clone the repo
git clone <repo-url>
cd familycare/backend

# Restore packages
dotnet restore

# Spin up Postgres + pgAdmin + API
docker compose up -d

# Run tests
dotnet test
```

See the main [README](README.md) for endpoint URLs and credentials.

## Coding conventions

- **Language**: all code, comments, commit messages and documentation are written in **English**.
- **Architecture**: Clean Architecture + DDD + CQRS. Respect layer boundaries (Domain → Application → Infrastructure → API).
- **Tests**: new features should be accompanied by unit tests (Domain/Application) and, when relevant, integration tests (API).
- **Naming**:
  - PascalCase for types, methods, properties.
  - camelCase for parameters and local variables.
  - snake_case for database columns (handled automatically by `EFCore.NamingConventions`).
- **Async**: methods that perform IO must be `async` and accept a `CancellationToken`.
- **Nullable**: `<Nullable>enable</Nullable>` is on globally. Resolve all warnings — they are errors via `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.

## Commit messages

Follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short description>

[optional body]

[optional footer]
```

Common types:

| Type      | Use for                                                      |
| --------- | ------------------------------------------------------------ |
| `feat`    | A new feature                                                |
| `fix`     | A bug fix                                                    |
| `refactor`| A change that neither adds a feature nor fixes a bug         |
| `perf`    | A performance improvement                                    |
| `test`    | Adding or updating tests                                     |
| `docs`    | Documentation only                                           |
| `chore`   | Tooling, build, dependencies                                 |
| `ci`      | Changes to CI configuration                                  |

Examples:

```
feat(family): add transfer ownership endpoint
fix(privacy): correct HashSet materialization for AllowedMemberIds
test(domain): cover invitation lifecycle invariants
docs(readme): document JWT key configuration
```

## Branching

- `main` — production-ready code. Protected.
- `develop` — integration branch for upcoming releases.
- `feat/<short-name>`, `fix/<short-name>` — feature/bugfix branches off `develop`.

## Pull requests

1. Branch off `develop`.
2. Make your changes with focused commits.
3. Run `dotnet build` and `dotnet test` locally before pushing.
4. Open a PR against `develop`.
5. CI (GitHub Actions) must be green.
6. At least one approval before merge.

## Database migrations

When you change a `DbContext` or entity configuration, generate a migration:

```bash
dotnet ef migrations add <DescriptiveName> \
  --project src/FamilyCare.Infrastructure \
  --startup-project src/FamilyCare.Api \
  --output-dir Persistence/Migrations
```

Migrations are applied automatically at API startup with retry/backoff.

## Reporting bugs

Open an issue with:

- What you expected to happen.
- What actually happened.
- Minimal reproduction steps.
- Relevant log output (use the `X-Correlation-Id` header to filter your request).
