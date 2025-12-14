# BackTrack - Lost & Found Platform

Microservices-based lost and found system with Firebase authentication, real-time chat, and push notifications.

## Overview

BackTrack is a platform for posting and finding lost items. Users can create posts for lost/found items, search and match items, chat with other users, and receive notifications.

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                          Clients                            │
│         (Mobile Apps, Web Apps, Third-party Apps)          │
└────────────────────────┬────────────────────────────────────┘
                         │
                         │ Firebase ID Token
                         ▼
            ┌────────────────────────┐
            │     API Gateway        │ ← Firebase Auth
            │   (YARP, .NET 8)       │ ← Correlation ID
            └─────────┬──────────────┘ ← Request Routing
                      │
        ┌─────────────┼─────────────┬────────────────┐
        │             │             │                │
        ▼             ▼             ▼                ▼
  ┌─────────┐  ┌──────────┐  ┌──────────┐  ┌──────────────┐
  │  Core   │  │   Chat   │  │ Notify   │  │   Future     │
  │ Service │  │ Service  │  │ Service  │  │  Services    │
  └────┬────┘  └────┬─────┘  └────┬─────┘  └──────────────┘
       │            │             │
       ▼            ▼             ▼
  ┌─────────┐  ┌──────────┐  ┌──────────┐
  │   DB    │  │ MongoDB  │  │   DB     │
  │(SQL/PG) │  │          │  │(SQL/PG)  │
  └─────────┘  └──────────┘  └──────────┘
```

### Services

| Service | Technology | Port | Purpose | Status |
|---------|-----------|------|---------|--------|
| **API Gateway** | .NET 8, YARP | 5000 | Auth, routing, correlation | ✅ Complete |
| **Core Service** | .NET 8, Clean Architecture | 8080 | Posts, search, matching | 🚧 In Progress |
| **Chat Service** | Node.js | 3000 | Real-time messaging | 📝 Planned |
| **Notifications** | .NET 8 | 7000 | Push notifications (FCM) | 📝 Planned |

Each service has its own database and can be deployed independently.

## Technology Stack

| Layer | Technology |
|-------|------------|
| **API Gateway** | .NET 8, YARP 2.2.0, Firebase Admin SDK |
| **Backend Services** | .NET 8 (Clean Architecture, CQRS, MediatR), Node.js 18+ |
| **Databases** | PostgreSQL, MongoDB |
| **Authentication** | Firebase Authentication |
| **Real-time** | SignalR / Socket.io |
| **Notifications** | Firebase Cloud Messaging (FCM) |
| **Containerization** | Docker, Docker Compose |

### Core Service Architecture

The Core Service follows Clean Architecture with CQRS pattern:

- **Contract Layer**: Request/Response DTOs with FluentValidation
- **Application Layer**: Commands, Queries, and MediatR Handlers
- **Domain Layer**: Entities, Value Objects, Domain Logic
- **Infrastructure Layer**: Database, Repositories, External Services
- **WebApi Layer**: Controllers, Middleware, Configuration

## Project Structure

```
service-backtrack/
├── Backtrack.ApiGateway/              # API Gateway service
│   ├── Middleware/                    # Auth & correlation middlewares
│   └── examples/                      # Auth client, token helpers
├── Backtrack.Core/                    # Core service (Clean Architecture)
│   ├── Backtrack.Core.WebApi/         # Controllers, Middleware
│   ├── Backtrack.Core.Application/    # Commands, Queries, Handlers
│   ├── Backtrack.Core.Contract/       # Request/Response DTOs
│   ├── Backtrack.Core.Domain/         # Entities, Value Objects
│   └── Backtrack.Core.Infrastructure/ # Database, Repositories
├── BackTrack.Chat/                    # Chat service (planned)
├── BackTrack.Notifications/           # Notifications service (planned)
├── docker-compose.yml                 # Orchestration
├── .env.example                       # Environment template
└── README.md
```

## Quick Start

### Prerequisites

- .NET 8 SDK
- Node.js 18+ (for Chat service)
- Docker & Docker Compose (optional)
- Firebase project

### 1. Clone and Setup

```bash
git clone <repository-url>
cd service-backtrack

# Copy environment template
cp .env.example .env
# Edit .env with your configuration
```

### 2. Firebase Setup

1. Create Firebase project at [Firebase Console](https://console.firebase.google.com/)
2. Enable Authentication methods (Email/Password, Google Sign-In)
3. Download service account:
   - Project Settings > Service Accounts > Generate New Private Key
   - Save as `firebase-service-account.json` in project root (**DO NOT commit!**)

### 3. Run with Docker Compose

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

Services will be available at:
- API Gateway: http://localhost:5000
- Core Service: http://localhost:8080

### 4. Run Locally (Development)

```bash
# Terminal 1: API Gateway
cd Backtrack.ApiGateway
export GOOGLE_APPLICATION_CREDENTIALS="../firebase-service-account.json"
dotnet run

# Terminal 2: Core Service
cd Backtrack.Core/Backtrack.Core.WebApi
dotnet run --urls "http://localhost:8080"
```

## Configuration

### Environment Variables

Create `.env` file (see `.env.example`):

```env
# Firebase
FIREBASE_PROJECT_ID=your-firebase-project-id

# Database connections
CORE_DB_CONNECTION=Host=localhost;Database=backtrack_core;Username=postgres;Password=yourpassword
CHAT_DB_CONNECTION=mongodb://localhost:27017/backtrack_chat
NOTIFY_DB_CONNECTION=Host=localhost;Database=backtrack_notifications;Username=postgres;Password=yourpassword
```

### Service Configuration

Each service has its own `appsettings.json`:
- API Gateway: `Backtrack.ApiGateway/appsettings.json`
- Core Service: `Backtrack.Core/Backtrack.Core.WebApi/appsettings.json`

---

**Current Status:** API Gateway ✅ | Core Service 🚧 | Chat Service 📝 | Notifications 📝
