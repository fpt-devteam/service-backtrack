# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Services Overview

BackTrack is a lost-and-found platform composed of 5 microservices:

| Service | Tech | Port (local) |
|---|---|---|
| `Backtrack.ApiGateway` | .NET 8, YARP | 5000 |
| `Backtrack.Core` | .NET 8, Clean Architecture | 8080 |
| `Backtrack.Chat` | Node.js/TypeScript | 3000 |
| `Backtrack.QR` | Node.js/TypeScript | 4000 |
| `Backtrack.Notification` | Node.js/TypeScript | 5000 |

All services are orchestrated via `docker-compose.yml` at the repo root.

## Commands

### Backtrack.Core (C#)

```bash
# Build
dotnet build Backtrack.Core.sln

# Run
dotnet run --project Backtrack.Core.WebApi
dotnet run --project Backtrack.Core.WebApi --urls "http://localhost:8080"

# EF Core migrations (run from Backtrack.Core/Backtrack.Core.Infrastructure/)
dotnet ef migrations add <MigrationName> --startup-project ..\Backtrack.Core.WebApi
dotnet ef database update --startup-project ..\Backtrack.Core.WebApi
dotnet ef migrations remove --startup-project ..\Backtrack.Core.WebApi
```

PowerShell migration helper scripts are in `Backtrack.Core.Infrastructure/Migrations/Scripts/`.

### Backtrack.Chat / Backtrack.QR / Backtrack.Notification (Node.js)

```bash
npm run dev       # Development with hot reload
npm run build     # TypeScript compilation
npm start         # Start production build
npm run lint      # ESLint with fix
npm run format    # Prettier formatting
```

`Backtrack.QR` also has `npm run seed:plans` to seed subscription plan data.

### Docker (full stack)

```bash
docker-compose up --build
```

Environment files per service are in `env/`.

## Architecture

### API Gateway

Single entry point for all external traffic. Validates Firebase ID tokens, then injects user context as HTTP headers for downstream services:
- `X-Auth-Id`, `X-Auth-Email`, `X-Auth-Name`, `X-Auth-Avatar-Url`, `X-Correlation-Id`

YARP routes requests:
- `/api/core/**` → Core Service
- `/api/chat/**` → Chat Service
- `/api/qr/**` → QR Service

### Backtrack.Core — Clean Architecture

Dependency flows inward: `WebApi → Application → Domain`, with `Infrastructure` implementing `Application` interfaces.

```
Backtrack.Core.Domain        # Entities, value objects, enums — no dependencies
Backtrack.Core.Application   # CQRS handlers, repository interfaces, exceptions
Backtrack.Core.Infrastructure # EF Core, repositories, RabbitMQ, Redis, Hangfire, AI
Backtrack.Core.Contract      # Request/Response DTOs
Backtrack.Core.WebApi        # Controllers, middleware, DI entry point
```

CQRS structure: `Usecases/{Feature}/Commands|Queries/{OperationName}/` — each operation folder contains Command/Query record, Handler, and optional FluentValidation Validator. Repository interfaces are co-located with their feature (e.g., `Usecases/Posts/IPostRepository.cs`).

Key conventions:
- **Response envelope**: All responses use `ApiResponse<T>`. Controllers call `this.ApiOk(result)` / `this.ApiCreated(result)` (extension methods in `ControllerExtensions.cs`).
- **Auth**: Controllers extract auth headers and enrich commands via `with` expressions (`command with { AuthorId = userId }`).
- **Errors**: Custom exceptions extend `DomainException` (carries `Error` record with `Code`/`Message`). `ExceptionHandlingMiddleware` maps them: `NotFoundException`→404, `ConflictException`→409, `ValidationException`→400, `ForbiddenException`→403. Predefined error constants in `Application/Exceptions/Errors/`.
- **Entities**: Base class `Entity<TKey>` provides `Id`, `CreatedAt`, `UpdatedAt`, `DeletedAt` (soft delete via partial unique indexes).
- **Pagination**: `PagedQuery.FromPage(page, pageSize)` → `PagedResult<T>`.
- **RBAC**: Authorization checks happen inside handlers, not middleware. Loads `Membership` entity and checks `Role` (OrgAdmin/OrgStaff).
- **Inter-service events**: CAP library + RabbitMQ. Publisher: `CapEventPublisher`. E.g., `UserEnsureExistIntegrationEvent`.

Tech: .NET 8, PostgreSQL + EF Core 8 + NetTopologySuite (spatial) + pgvector, MediatR 11, FluentValidation 11, Hangfire, Redis, Semantic Kernel, Azure Document Intelligence.

### Node.js Services (Chat, QR, Notification)

All three follow the same layered structure:
```
src/
  controllers/   # Route handlers
  services/      # Business logic
  repositories/  # Data access
  models/        # MongoDB schemas
  routes/        # Express router definitions
  middlewares/
  utils/
```

- **Databases**: MongoDB (separate instance per service, ports 27017–27019)
- **Messaging**: RabbitMQ consumers/publishers for cross-service events (e.g., user sync)
- **Real-time**: Backtrack.Chat uses Socket.io for WebSocket messaging
- **Result pattern**: Type-safe error handling without thrown exceptions

### Infrastructure

- **Databases**: PostgreSQL 14 with PostGIS + pgvector for Core; MongoDB for Chat/QR/Notification
- **Message broker**: RabbitMQ 3.13 (AMQP :5672, management UI :15672)
- **Auth**: Firebase Authentication (token validation at gateway)
- **Notifications**: Firebase Cloud Messaging
- **Caching**: Redis
