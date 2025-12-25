# Backtrack.QR - Item & QR Code Management Service

A microservice within the Backtrack platform responsible for managing items and their associated QR codes. This service provides RESTful APIs for creating, retrieving, and managing items with automatically generated QR codes, enabling item tracking and identification.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Database Models](#database-models)
- [Event-Driven Architecture](#event-driven-architecture)
- [Development](#development)
- [Error Handling](#error-handling)

## Overview

Backtrack.QR is a Node.js/TypeScript-based microservice that handles:
- Item creation and management
- Automatic QR code generation and linking
- User synchronization via RabbitMQ messaging
- RESTful API endpoints for item operations

The service is designed to work within a microservices architecture, communicating with other Backtrack services through an API Gateway and RabbitMQ message broker.

## Features

- **Item Management**: Create, retrieve, and list items with rich metadata
- **Automatic QR Code Generation**: Unique QR codes automatically generated for each item
- **Transaction Support**: MongoDB transactions ensure data consistency
- **Event-Driven**: Consumes user events from RabbitMQ for user synchronization
- **Type Safety**: Full TypeScript implementation with Zod validation
- **Error Handling**: Comprehensive error handling with custom error catalog
- **Logging**: Structured logging with Winston
- **Health Checks**: Built-in health check endpoint
- **Correlation IDs**: Request tracking across services
- **Graceful Shutdown**: Proper cleanup of resources on shutdown

## Tech Stack

### Core Technologies
- **Runtime**: Node.js (ES2022, ESM modules)
- **Language**: TypeScript 5.9+
- **Framework**: Express 5.2+
- **Database**: MongoDB 9.0+ with Mongoose
- **Message Broker**: RabbitMQ (amqplib)

### Key Dependencies
- **Validation**: Zod 4.2+
- **Logging**: Winston 3.19+
- **Security**: Helmet 8.1+
- **Error Handling**: express-async-handler 1.2+
- **Environment**: dotenv 17.2+

### Development Tools
- **TypeScript Compiler**: tsc & tsc-alias
- **Dev Server**: tsx & nodemon
- **Module System**: NodeNext (ESM)

## Architecture

### System Context

```
┌─────────────────┐
│  API Gateway    │
│   (Port 8080)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│ Backtrack.QR    │◄────►│   MongoDB    │
│  (Port 4000)    │      │              │
└────────┬────────┘      └──────────────┘
         │
         ▼
┌─────────────────┐
│   RabbitMQ      │
│  Message Broker │
└─────────────────┘
```

### Request Flow

1. **Client** → API Gateway (Port 8080)
2. **API Gateway** → Backtrack.QR Service (Port 4000)
3. **Backtrack.QR** → MongoDB (Data persistence)
4. **RabbitMQ** → Backtrack.QR (User sync events)

## Project Structure

```
Backtrack.QR/
├── src/
│   ├── configs/                 # Configuration files
│   │   ├── constants.ts         # Application constants
│   │   └── env.ts               # Environment validation with Zod
│   │
│   ├── consumers/               # RabbitMQ message consumers
│   │   └── user-sync.consumer.ts
│   │
│   ├── contracts/               # DTOs and mappers
│   │   ├── common/              # Shared contracts
│   │   │   ├── api-response.ts  # Standardized API responses
│   │   │   └── pagination.ts    # Pagination utilities
│   │   ├── events/              # Event definitions
│   │   │   ├── event-topics.ts
│   │   │   └── user-events.ts
│   │   ├── item/                # Item contracts
│   │   │   ├── item.mapper.ts
│   │   │   ├── item.request.ts
│   │   │   └── item.response.ts
│   │   └── qr-code/             # QR code contracts
│   │       ├── qr-code.mapper.ts
│   │       └── qr-code.response.ts
│   │
│   ├── controllers/             # HTTP request handlers
│   │   └── item.controller.ts
│   │
│   ├── database/                # Database layer
│   │   ├── models/              # Mongoose models
│   │   │   ├── item.model.ts
│   │   │   ├── qr-code.models.ts
│   │   │   └── user.model.ts
│   │   ├── view/                # View models/DTOs
│   │   │   └── item.view.ts
│   │   └── connect.ts           # Database connection
│   │
│   ├── errors/                  # Error definitions
│   │   ├── catalog/             # Error catalogs
│   │   │   ├── item.error.ts
│   │   │   └── qr-code.error.ts
│   │   └── error.ts             # Base error types
│   │
│   ├── messaging/               # RabbitMQ infrastructure
│   │   ├── consumer-manager.ts
│   │   └── rabbitmq-connection.ts
│   │
│   ├── middlewares/             # Express middlewares
│   │   ├── correlation.middleware.ts
│   │   ├── error.middleware.ts
│   │   └── logging.middleware.ts
│   │
│   ├── repositories/            # Data access layer
│   │   ├── item.repository.ts
│   │   ├── qr-code.repository.ts
│   │   └── user.repository.ts
│   │
│   ├── routes/                  # Route definitions
│   │   └── item.route.ts
│   │
│   ├── services/                # Business logic layer
│   │   └── item.service.ts
│   │
│   ├── utils/                   # Utility functions
│   │   ├── headers.ts           # HTTP header constants
│   │   ├── logger.ts            # Winston logger configuration
│   │   ├── object-id.ts         # MongoDB ObjectId utilities
│   │   ├── qr-code-generator.ts # QR code generation
│   │   └── result.ts            # Result pattern implementation
│   │
│   ├── index.ts                 # Application entry point
│   ├── main.ts                  # Server bootstrap
│   └── server.ts                # Express app configuration
│
├── dist/                        # Compiled JavaScript (build output)
├── node_modules/                # Dependencies
├── package.json                 # Project metadata & dependencies
├── tsconfig.json               # TypeScript configuration
└── README.md                   # This file
```

### Layer Responsibilities

- **Controllers**: Handle HTTP requests/responses, validation
- **Services**: Business logic, orchestration, transactions
- **Repositories**: Data access, database queries
- **Models**: Database schemas and models
- **Contracts**: Request/response DTOs, mappers
- **Middlewares**: Cross-cutting concerns (logging, errors, correlation)
- **Utils**: Reusable helper functions

## Prerequisites

- **Node.js**: v18+ (ES2022 support required)
- **npm**: v9+ or compatible package manager
- **MongoDB**: v7+ (for local development)
- **RabbitMQ**: v3.13+ (for message broker)
- **Docker** (optional): For containerized deployment

## Installation

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd service-backtrack/Backtrack.QR
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Configure environment variables**:
   Create environment file at `../env/backtrack-qr-api.docker.env`:
   ```env
   NODE_ENV=development
   PORT=4000

   # Database
   DATABASE_URI=mongodb://localhost:27017/backtrack-qr

   # RabbitMQ
   RABBITMQ_URL=amqp://backtrack:backtrack123@localhost:5672/
   RABBITMQ_EXCHANGE=backtrack-events
   RABBITMQ_USER_SYNC_QUEUE=qr-service.user-sync

   # Logging
   LOG_LEVEL=info
   ```

## Configuration

### Environment Variables

The service uses Zod for runtime environment validation. All configuration is defined in `src/configs/env.ts`:

| Variable | Type | Required | Default | Description |
|----------|------|----------|---------|-------------|
| `NODE_ENV` | enum | No | `development` | Environment: development, test, production |
| `PORT` | number | No | `3000` | HTTP server port |
| `DATABASE_URI` | string | Yes | - | MongoDB connection string |
| `RABBITMQ_URL` | string | Yes | - | RabbitMQ connection URL |
| `RABBITMQ_EXCHANGE` | string | Yes | - | RabbitMQ exchange name |
| `RABBITMQ_USER_SYNC_QUEUE` | string | Yes | - | Queue name for user sync events |
| `LOG_LEVEL` | enum | No | `info` | Logging level: debug, info, warn, error |

### TypeScript Configuration

The project uses modern TypeScript settings:
- **Module System**: ESM (NodeNext)
- **Target**: ES2022
- **Path Aliases**: `@/src/*` → `./src/*`
- **Strict Mode**: Enabled
- **Source Maps**: Enabled

## Running the Application

### Development Mode

```bash
# Run with hot reload
npm run dev
```

This uses `nodemon` with `tsx` for TypeScript execution and automatic restart on file changes.

### Production Build

```bash
# Build the project
npm run build

# Run built application
npm start
```

The build process:
1. Compiles TypeScript to JavaScript (`tsc`)
2. Resolves path aliases (`tsc-alias`)
3. Outputs to `dist/` directory

### Docker Deployment

```bash
# Build Docker image
docker build -f ../dockerfiles/Dockerfile.backtrack-qr-api -t backtrack-qr-api .

# Run with Docker Compose (from root)
cd ..
docker-compose up backtrack-qr-api
```

### Health Check

```bash
curl http://localhost:4000/health
```

Expected response:
```json
{
  "status": "healthy"
}
```

## API Documentation

### Base URL

- **Development**: `http://localhost:4000`
- **Production**: `http://<api-gateway>:8080/qr` (via API Gateway)

### Authentication

All endpoints require the `X-Auth-Id` header containing the user's ID:

```
X-Auth-Id: <user-id>
```

### Endpoints

#### 1. Get All Items

Retrieve paginated list of items owned by the authenticated user.

**Request:**
```http
GET /items?page=1&pageSize=10
X-Auth-Id: user123
```

**Query Parameters:**
- `page` (optional): Page number (default: 1, min: 1)
- `pageSize` (optional): Items per page (default: 10, min: 1, max: 100)

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "676b6a7c8d9e0f1234567890",
        "name": "Lost Wallet",
        "description": "Brown leather wallet",
        "imageUrls": ["https://example.com/image.jpg"],
        "ownerId": "user123",
        "qrCode": {
          "id": "676b6a7c8d9e0f1234567891",
          "publicCode": "ABC123XYZ",
          "status": "linked",
          "linkedAt": "2024-12-25T10:30:00.000Z"
        },
        "createdAt": "2024-12-25T10:30:00.000Z",
        "updatedAt": "2024-12-25T10:30:00.000Z"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

#### 2. Get Item by ID

Retrieve a specific item with its QR code.

**Request:**
```http
GET /items/:id
X-Auth-Id: user123
```

**Path Parameters:**
- `id`: MongoDB ObjectId of the item

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "676b6a7c8d9e0f1234567890",
    "name": "Lost Wallet",
    "description": "Brown leather wallet",
    "imageUrls": ["https://example.com/image.jpg"],
    "ownerId": "user123",
    "qrCode": {
      "id": "676b6a7c8d9e0f1234567891",
      "publicCode": "ABC123XYZ",
      "status": "linked",
      "linkedAt": "2024-12-25T10:30:00.000Z"
    },
    "createdAt": "2024-12-25T10:30:00.000Z",
    "updatedAt": "2024-12-25T10:30:00.000Z"
  }
}
```

**Error Response (404):**
```json
{
  "success": false,
  "error": {
    "code": "ItemNotFound",
    "message": "Item not found"
  }
}
```

#### 3. Create Item

Create a new item with an automatically generated QR code.

**Request:**
```http
POST /items
Content-Type: application/json
X-Auth-Id: user123

{
  "name": "Lost Wallet",
  "description": "Brown leather wallet with cards",
  "imageUrls": [
    "https://example.com/wallet-front.jpg",
    "https://example.com/wallet-back.jpg"
  ]
}
```

**Request Body:**
- `name` (required): Item name (non-empty string)
- `description` (optional): Item description
- `imageUrls` (optional): Array of image URLs

**Response (201 Created):**
```json
{
  "success": true,
  "data": {
    "id": "676b6a7c8d9e0f1234567890",
    "name": "Lost Wallet",
    "description": "Brown leather wallet with cards",
    "imageUrls": [
      "https://example.com/wallet-front.jpg",
      "https://example.com/wallet-back.jpg"
    ],
    "ownerId": "user123",
    "qrCode": {
      "id": "676b6a7c8d9e0f1234567891",
      "publicCode": "ABC123XYZ",
      "status": "linked",
      "linkedAt": "2024-12-25T10:30:00.000Z"
    },
    "createdAt": "2024-12-25T10:30:00.000Z",
    "updatedAt": "2024-12-25T10:30:00.000Z"
  }
}
```

**Error Response (400):**
```json
{
  "success": false,
  "error": {
    "code": "InvalidItemName",
    "message": "Item name cannot be empty"
  }
}
```

### Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `ItemNotFound` | 404 | Item does not exist |
| `InvalidItemName` | 400 | Item name is invalid or empty |
| `QrCodeNotFound` | 404 | Associated QR code not found |
| `QrCodeGenerationFailed` | 500 | Failed to generate unique QR code |

## Database Models

### Item Model

```typescript
{
  _id: ObjectId,                    // Auto-generated
  name: string,                     // Required
  description?: string,             // Optional
  imageUrls?: string[],             // Optional
  ownerId: string,                  // Required, user ID
  createdAt: Date,                  // Auto-generated
  updatedAt: Date                   // Auto-updated
}
```

**Indexes:**
- `ownerId` (for efficient user queries)

### QR Code Model

```typescript
{
  _id: ObjectId,                    // Auto-generated
  publicCode: string,               // Required, unique 9-char code
  ownerId: string,                  // Required, user ID
  itemId?: string,                  // Optional, linked item ID
  status: 'unlinked' | 'linked',    // Required
  linkedAt?: Date,                  // Set when status = 'linked'
  createdAt: Date,                  // Auto-generated
  updatedAt: Date                   // Auto-updated
}
```

**Indexes:**
- `publicCode` (unique, for QR code lookups)
- `itemId` (for item-to-QR queries)
- `ownerId` (for user queries)

### User Model

```typescript
{
  _id: ObjectId,                    // Auto-generated
  userId: string,                   // Required, unique, external user ID
  email?: string,                   // Optional
  displayName?: string,             // Optional
  photoUrl?: string,                // Optional
  createdAt: Date,                  // Auto-generated
  updatedAt: Date                   // Auto-updated
}
```

**Indexes:**
- `userId` (unique, for user lookups)

## Event-Driven Architecture

### RabbitMQ Integration

The service consumes events from RabbitMQ to maintain eventual consistency across the platform.

**Configuration:**
- **Exchange**: `backtrack-events` (topic exchange)
- **Queue**: `qr-service.user-sync`
- **Routing Key**: `user.*`

### User Sync Consumer

Listens for user-related events to keep the local user cache in sync.

**Event Types:**

1. **User Created**
   ```json
   {
     "eventType": "UserCreated",
     "userId": "user123",
     "email": "user@example.com",
     "displayName": "John Doe",
     "photoUrl": "https://example.com/photo.jpg",
     "timestamp": "2024-12-25T10:30:00.000Z"
   }
   ```

2. **User Updated**
   ```json
   {
     "eventType": "UserUpdated",
     "userId": "user123",
     "email": "newemail@example.com",
     "displayName": "John Smith",
     "photoUrl": "https://example.com/newphoto.jpg",
     "timestamp": "2024-12-25T10:30:00.000Z"
   }
   ```

**Consumer Behavior:**
- Creates or updates user in local database
- Maintains idempotency
- Logs processing for observability

## Development

### Code Organization

The project follows a **layered architecture**:

1. **Presentation Layer** (Controllers, Routes, Middlewares)
2. **Business Layer** (Services)
3. **Data Access Layer** (Repositories, Models)
4. **Cross-Cutting** (Utils, Errors, Contracts)

### Design Patterns

- **Result Pattern**: Type-safe error handling without exceptions
- **Repository Pattern**: Abstract data access
- **Dependency Injection**: Through module imports
- **DTO/Mapper Pattern**: Separate internal models from API contracts

### Result Pattern

```typescript
type Result<T> = Success<T> | Failure;

// Usage
const result = await ItemService.getByIdAsync(id);

if (isSuccess(result)) {
  const item = result.value;
  // Handle success
} else {
  const error = result.error;
  // Handle error
}
```

### Adding New Features

1. **Define Contracts** (`contracts/`)
   - Request/Response DTOs
   - Mappers

2. **Create Database Model** (`database/models/`)
   - Mongoose schema
   - Indexes

3. **Implement Repository** (`repositories/`)
   - Data access methods

4. **Implement Service** (`services/`)
   - Business logic
   - Use transactions if needed

5. **Create Controller** (`controllers/`)
   - HTTP handlers
   - Validation

6. **Define Routes** (`routes/`)
   - HTTP method mapping
   - Middleware application

7. **Add Error Definitions** (`errors/catalog/`)
   - Custom error types

### Testing

```bash
# Run tests (when implemented)
npm test
```

### Linting & Formatting

```bash
# Type checking
npx tsc --noEmit

# Check for issues
# (Add ESLint/Prettier as needed)
```

## Error Handling

### Error Structure

All errors follow a consistent structure:

```typescript
type AppError = {
  kind: 'NotFound' | 'BadRequest' | 'Internal';
  code: string;        // Machine-readable error code
  message: string;     // Human-readable message
};
```

### HTTP Status Mapping

- `NotFound` → 404
- `BadRequest` → 400
- `Internal` → 500

### Global Error Middleware

The `errorMiddleware` catches all unhandled errors and formats them consistently:

```typescript
app.use(errorMiddleware);
```

### Best Practices

1. **Use Result Pattern**: Return `Result<T>` instead of throwing
2. **Define Error Catalogs**: Centralize error definitions
3. **Log Errors**: Use structured logging with Winston
4. **Include Context**: Add correlation IDs for tracing

---

## Contributing

1. Follow the existing code structure and patterns
2. Use TypeScript strict mode
3. Add appropriate error handling
4. Update documentation for new features
5. Ensure all services use the Result pattern

## License

ISC

## Support

For issues or questions, please contact the development team or create an issue in the repository.
