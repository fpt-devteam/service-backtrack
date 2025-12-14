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
| **Core Service** | .NET 8 | 8080 | Posts, search, matching | 📝 Planned |
| **Chat Service** | Node.js | 3000 | Real-time messaging | 📝 Planned |
| **Notifications** | .NET 8 | 7000 | Push notifications (FCM) | 📝 Planned |

Each service has its own database and can be deployed independently.

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
2. Enable Authentication methods:
   - Email/Password
   - Google Sign-In
3. Create test users in Firebase Console > Authentication > Users
4. Download service account:
   - Project Settings > Service Accounts
   - Generate New Private Key
   - Save as `firebase-service-account.json` in project root (DO NOT commit!)

### 3. Run with Docker Compose (Recommended)

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
- Chat Service: http://localhost:3000
- Notifications: http://localhost:7000

### 4. Run Locally (Development)

```bash
# Terminal 1: API Gateway
cd Backtrack.ApiGateway
export GOOGLE_APPLICATION_CREDENTIALS="../firebase-service-account.json"
dotnet run

# Terminal 2: Core Service (when implemented)
# cd BackTrack.Core
# dotnet run --urls "http://localhost:8080"

# Terminal 3: Chat Service (when implemented)
# cd BackTrack.Chat
# npm run dev

# Terminal 4: Notifications Service (when implemented)
# cd BackTrack.Notifications
# dotnet run --urls "http://localhost:7000"
```

## Authentication & Testing

### Get Firebase Token

**Option 1: HTML Auth Client**
```bash
# Open with VS Code Live Server
open Backtrack.ApiGateway/examples/test-client.html
# Login with Email/Password or Google
# Copy token to clipboard
```

**Option 2: Node.js CLI**
```bash
cd Backtrack.ApiGateway/examples
npm install
node get-firebase-token.js your-email@example.com your-password
```

### Test with Postman

1. Create Postman environment:
   - `GATEWAY_URL`: `http://localhost:5000`
   - `FIREBASE_TOKEN`: `<your-token>`

2. Test endpoints:
   ```
   GET  {{GATEWAY_URL}}/health                    # Health check
   GET  {{GATEWAY_URL}}/api/core/posts            # List posts
   POST {{GATEWAY_URL}}/api/core/posts            # Create post
   GET  {{GATEWAY_URL}}/api/chat/conversations    # Conversations
   ```

3. Add to all authenticated requests:
   - Header: `Authorization: Bearer {{FIREBASE_TOKEN}}`

## API Routes

| Route | Service | Auth | Description |
|-------|---------|------|-------------|
| `GET /health` | Gateway | No | Health check |
| `GET /public/**` | Various | No | Public endpoints |
| `/api/core/**` | Core | Yes | Posts, search, matching |
| `/api/chat/**` | Chat | Yes | Real-time messaging |
| `/api/notify/**` | Notifications | Yes | Push notifications |

Gateway validates Firebase tokens and injects user headers (`X-User-Id`, `X-User-Email`, etc.) to downstream services.

## Project Structure

```
service-backtrack/
├── Backtrack.ApiGateway/           # API Gateway service
│   ├── Middleware/                 # Auth & correlation middlewares
│   ├── examples/                   # Auth client, token helpers
│   ├── Dockerfile
│   └── README.md                   # Gateway documentation
├── BackTrack.Core/                 # Core service (planned)
│   └── README.md
├── BackTrack.Chat/                 # Chat service (planned)
│   └── README.md
├── BackTrack.Notifications/        # Notifications service (planned)
│   └── README.md
├── docker-compose.yml              # Orchestration
├── .env.example                    # Environment template
├── .gitignore
└── README.md                       # This file
```

## Configuration

### Environment Variables

Create `.env` file (see `.env.example`):

```env
# Firebase
FIREBASE_PROJECT_ID=your-firebase-project-id

# Database connections
CORE_DB_CONNECTION=Host=localhost;Database=backtrack_core;...
CHAT_DB_CONNECTION=mongodb://localhost:27017/backtrack_chat
NOTIFY_DB_CONNECTION=Host=localhost;Database=backtrack_notifications;...
```

### Service-Specific Configuration

Each service has its own configuration file:
- API Gateway: `Backtrack.ApiGateway/appsettings.json`
- Core Service: `BackTrack.Core/appsettings.json`
- Chat Service: `BackTrack.Chat/.env`
- Notifications: `BackTrack.Notifications/appsettings.json`

See individual service READMEs for details.

## Development Workflow

1. **Start Services**: Run docker-compose or individual services locally
2. **Get Token**: Use auth client or CLI to get Firebase token
3. **Test APIs**: Use Postman with Firebase token in headers
4. **Check Logs**: View correlation IDs across services
5. **Iterate**: Make changes, services auto-reload in dev mode

## Service Documentation

- [API Gateway](Backtrack.ApiGateway/README.md) - Firebase auth, routing, YARP configuration
- [Auth Client Guide](Backtrack.ApiGateway/examples/README.md) - Token generation, Postman setup
- Core Service - (To be implemented)
- Chat Service - (To be implemented)
- Notifications Service - (To be implemented)

## Technology Stack

| Layer | Technology |
|-------|------------|
| **API Gateway** | .NET 8, YARP 2.2.0, Firebase Admin SDK 3.0.1 |
| **Backend Services** | .NET 8, Node.js 18+ |
| **Databases** | PostgreSQL, MongoDB |
| **Authentication** | Firebase Authentication |
| **Real-time** | SignalR / Socket.io |
| **Notifications** | Firebase Cloud Messaging (FCM) |
| **Containerization** | Docker, Docker Compose |

## Common Issues

### Firebase Authentication Failed
- Check `firebase-service-account.json` exists and is valid
- Verify `FIREBASE_PROJECT_ID` in `.env` matches Firebase project
- Ensure Firebase Authentication is enabled in console

### Service Not Available (503)
- Check if service is running: `docker-compose ps` or check terminal
- Verify port configuration matches service defaults
- Check service URLs in gateway configuration

### Token Expired (401)
- Firebase tokens expire after 1 hour
- Get fresh token from auth client or CLI
- Verify `Authorization: Bearer <token>` header format

### Database Connection Failed
- Check database is running
- Verify connection strings in `.env`
- Ensure database exists and credentials are correct

## Security Best Practices

✅ Never commit `firebase-service-account.json`
✅ Never commit `.env` file
✅ Use environment variables for sensitive config
✅ Tokens expire after 1 hour (automatic via Firebase)
✅ Gateway validates all tokens before routing
✅ Each service has isolated database
✅ HTTPS in production (configure reverse proxy)

## Deployment

### Local Development
```bash
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up
```

### Production
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Configure production environment:
- Use HTTPS (nginx/Traefik reverse proxy)
- Set production database URLs
- Configure proper Firebase service account
- Enable monitoring and logging
- Set up CI/CD pipeline

## Roadmap

### Phase 1: Foundation (Current)
- ✅ API Gateway with Firebase auth
- ✅ Correlation ID tracking
- ✅ Auth client for token generation
- ✅ Docker Compose setup

### Phase 2: Core Features
- ⬜ Core Service (Posts CRUD, Search, Matching)
- ⬜ Database setup (PostgreSQL)
- ⬜ API documentation (Swagger)

### Phase 3: Real-time Features
- ⬜ Chat Service (SignalR/Socket.io)
- ⬜ MongoDB setup
- ⬜ Real-time notifications

### Phase 4: Notifications
- ⬜ Notifications Service (FCM)
- ⬜ Push notification registration
- ⬜ Notification templates

### Phase 5: Production
- ⬜ Rate limiting
- ⬜ Caching layer (Redis)
- ⬜ Centralized logging (ELK/Seq)
- ⬜ Monitoring (Prometheus/Grafana)
- ⬜ CI/CD pipeline
- ⬜ Load testing

## Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Implement changes
3. Test locally with all services
4. Create pull request

## License

[Your License Here]

## Support

For issues and questions:
- Check service-specific READMEs
- Review common issues section
- Check logs with correlation IDs
- Review Firebase console for auth issues

---

**Current Status:** API Gateway ✅ | Core Service 📝 | Chat Service 📝 | Notifications 📝
