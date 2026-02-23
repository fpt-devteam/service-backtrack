# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build the solution
dotnet build Backtrack.Core.sln

# Run the WebApi project (from Backtrack.Core/)
dotnet run --project Backtrack.Core.WebApi

# Run locally on specific port
dotnet run --project Backtrack.Core.WebApi --urls "http://localhost:8080"
```

No test projects exist yet.

## EF Core Migrations

Run from `Backtrack.Core.Infrastructure/`:

```bash
# Add migration
dotnet ef migrations add <MigrationName> --startup-project ..\Backtrack.Core.WebApi

# Apply migrations
dotnet ef database update --startup-project ..\Backtrack.Core.WebApi

# Remove last unapplied migration
dotnet ef migrations remove --startup-project ..\Backtrack.Core.WebApi
```

PowerShell helper scripts are in `Backtrack.Core.Infrastructure/Migrations/Scripts/`.

## Architecture

Clean Architecture with 4 projects — dependency flows inward:

```
WebApi → Application → Domain
  └──────→ Infrastructure (implements Application interfaces)
```

- **Domain**: Entities (`Entity<TKey>` base with `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt` soft-delete), value objects, enums. No dependencies.
- **Application**: CQRS via MediatR. Commands/queries in `Usecases/{Feature}/Commands|Queries/{OperationName}/`. Each operation has its own folder with Command/Query, Handler, and optional Validator. Repository interfaces live alongside their feature (`Usecases/Posts/IPostRepository.cs`). FluentValidation + `ValidationBehavior` pipeline.
- **Infrastructure**: EF Core with PostgreSQL (`ApplicationDbContext`), repositories, AI services (Semantic Kernel), background jobs (Hangfire), messaging (CAP + RabbitMQ), Redis caching.
- **WebApi**: Controllers, middleware, DI configuration. Entry point is `Program.cs`.

## Key Conventions

### API Response Envelope
All responses use `ApiResponse<T>` wrapper. Controllers return `this.ApiOk(result)` or `this.ApiCreated(result)` (extension methods in `ControllerExtensions.cs`). Error responses use `ApiError` with `Code`, `Message`, `Details`.

### Auth via Gateway Headers
The API Gateway validates Firebase tokens and forwards identity as HTTP headers. Controllers read these headers to get caller identity:
- `X-Auth-Id`, `X-Auth-Email`, `X-Auth-Name`, `X-Auth-Avatar-Url`, `X-Correlation-Id`

Controllers set auth fields on commands using `with` expressions (e.g., `command with { AuthorId = userId }`).

### Error Handling
Custom exceptions in `Application/Exceptions/` map to HTTP status codes in `ExceptionHandlingMiddleware`:
- `NotFoundException` → 404, `ConflictException` → 409, `ValidationException` → 400, `ForbiddenException` → 403, `UnauthorizedException` → 401
- All extend `DomainException` which carries an `Error` record (`Code`, `Message`).
- Predefined error constants in `Application/Exceptions/Errors/` (e.g., `PostErrors`, `UserErrors`, `OrganizationErrors`).

### Pagination
`PagedQuery` (offset/limit) created via `PagedQuery.FromPage(page, pageSize)` in handlers. Results wrapped in `PagedResult<T>`.

### RBAC
Auth checks done in handlers (not middleware). Handlers load caller's `Membership` entity and check `Role` (OrgAdmin/OrgStaff). Last-admin constraint enforced in handlers.

### Entities
Base class `Entity<TKey>` provides `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt`. Core entities: `User`, `Post`, `Organization`, `Membership`. Soft-delete via `DeletedAt` with partial unique indexes.

### Inter-Service Messaging
CAP library with RabbitMQ for integration events (e.g., `UserEnsureExistIntegrationEvent`). Publisher: `CapEventPublisher` in Infrastructure.

## Tech Stack

- .NET 8, C# with nullable reference types
- PostgreSQL with EF Core 8, NetTopologySuite (spatial), pgvector (embeddings)
- MediatR 11, FluentValidation 11
- Hangfire (background jobs), Redis (caching)
- Semantic Kernel (AI/LLM), Azure Document Intelligence
- CAP + RabbitMQ (event bus)
- Docker support via `docker-compose.yml` in repo root
