## Backtrack Notification Service

A microservice for handling notifications in the Backtrack application.

## About

This service manages all notification-related operations including creating, sending, and tracking notifications across the platform.

## Features

- Notification creation and management
- RabbitMQ integration for event-driven notifications
- MongoDB database for notification persistence
- RESTful API endpoints
- Cursor-based pagination
- Error handling and logging

## Prerequisites

- Node.js >= 16.0.0
- MongoDB
- RabbitMQ

## Available Scripts

### `npm run clean-install`

Remove the existing `node_modules/` folder, `package-lock.json`, and reinstall all library modules.

### `npm run dev` or `npm run dev:hot` (hot reloading)

Run the server in development mode.

**IMPORTANT**: development mode uses `swc` for performance reasons which DOES NOT check for typescript errors. Run `npm run type-check` to check for type errors. NOTE: you should use your IDE to prevent most type errors.

### `npm run lint`

Check for linting errors.

### `npm run build`

Build the project for production.

### `npm start`

Run the production build (Must be built first).

### `npm run type-check`

Check for typescript errors.

## Environment Variables

Create an environment file at `../env/backtrack-notification-api.docker.env` with the following variables:

```env
NODE_ENV=development
PORT=3001
MONGODB_CONNECTIONSTRING=mongodb://localhost:27017/backtrack-notification
RABBITMQ_URL=amqp://localhost:5672
RABBITMQ_EXCHANGE=backtrack-exchange
RABBITMQ_NOTIFICATION_QUEUE=notification_queue
DEFAULT_PAGE_LIMIT=20
MAX_PAGE_LIMIT=100
```

## Project Structure

```
Backtrack.Notification/
├── src/
│   ├── common/
│   │   ├── constants/      # Application constants
│   │   └── errors/         # Custom error classes
│   ├── configs/            # Configuration files
│   ├── consumers/          # RabbitMQ consumers
│   ├── contracts/          # Request/Response interfaces
│   │   ├── events/         # Event definitions
│   │   ├── requests/       # Request DTOs
│   │   └── responses/      # Response DTOs
│   ├── controllers/        # Route controllers
│   ├── decorators/         # Custom decorators
│   ├── messaging/          # RabbitMQ connection management
│   ├── middlewares/        # Express middlewares
│   ├── models/             # Mongoose models
│   ├── repositories/       # Data access layer
│   │   └── base/          # Base repository classes
│   ├── routes/             # API routes
│   ├── scripts/            # Utility scripts
│   ├── services/           # Business logic
│   ├── utils/              # Helper functions
│   ├── index.ts            # Application entry point
│   └── server.ts           # Express server setup
├── dist/                   # Compiled output
├── eslint.config.ts        # ESLint configuration
├── package.json
├── tsconfig.json           # TypeScript configuration
└── tsconfig.prod.json      # Production TypeScript config
```

## API Endpoints

### Health Check
- `GET /health` - Check service health status

### Notifications (To be implemented)
- Add your notification routes here

## Development

1. Install dependencies:
```bash
npm install
```

2. Set up environment variables (see Environment Variables section)

3. Run in development mode:
```bash
npm run dev
```

4. Run with hot reloading:
```bash
npm run dev:hot
```

## Production

1. Build the project:
```bash
npm run build
```

2. Start the production server:
```bash
npm start
```

## Testing

To be implemented.

## Architecture

This service follows a layered architecture:
- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic
- **Repositories**: Handle data access
- **Models**: Define data schemas
- **Consumers**: Process RabbitMQ messages

## Additional Notes

- Always run `npm run type-check` before committing to ensure there are no TypeScript errors
- Use the provided error handling middleware for consistent error responses
- Follow the existing code patterns when adding new features
