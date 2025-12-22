# Correlation ID Flow

## Overview
Every request and response includes a unique `correlationId` for request tracing and debugging.

## Flow Architecture

```
Client Request
    ↓
[correlationIdMiddleware] → Generate or use existing correlationId
    ↓
req.headers['x-correlation-id'] = correlationId
    ↓
Controller → Read correlationId from headers
    ↓
Response (Success or Error) → Include same correlationId
```

## Implementation

### 1. Middleware (src/middlewares/correlationId.ts)
```typescript
// Generates correlationId for every request
export const correlationIdMiddleware = (req, res, next) => {
  // Use client's correlationId or generate new one
  const correlationId = req.headers['x-correlation-id'] || randomUUID();

  // Attach to request for controllers and error handler
  req.headers['x-correlation-id'] = correlationId;

  // Add to response headers for debugging
  res.setHeader('X-Correlation-Id', correlationId);

  next();
};
```

### 2. Server Setup (src/server.ts)
```typescript
// Add correlationId middleware BEFORE routes
app.use(correlationIdMiddleware);

// Then add routes
app.use('/api/conversations', conversationRoute);
app.use('/api/conversations', messageRoute);

// Error handler at the end
app.use(errorHandler);
```

### 3. Controllers
Controllers read `correlationId` from request headers:

```typescript
public async sendMessage(req: Request, res: Response) {
  const correlationId = req.headers['x-correlation-id'] as string;

  // ... business logic ...

  return res.json({
    success: true,
    data: message,
    correlationId, // Same ID for success
  });
}
```

### 4. Error Handler (src/middlewares/errorHandler.ts)
Error handler uses the SAME `correlationId`:

```typescript
export const errorHandler = (err, req, res, next) => {
  const correlationId = req.headers['x-correlation-id'] as string;

  return res.status(statusCode).json({
    success: false,
    error: {
      code: err.constructor.name,
      message: err.message
    },
    correlationId, // Same ID for errors
  });
};
```

## Benefits

### 1. Consistent Tracing
```json
// Request 1 - Success
{
  "success": true,
  "data": {...},
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}

// Request 1 - If it had failed
{
  "success": false,
  "error": {...},
  "correlationId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

### 2. Request Tracing
- Client can send `x-correlation-id` header for end-to-end tracing
- Server generates one if not provided
- Same ID appears in logs, responses, and errors

### 3. Debugging
```bash
# Find all logs for a specific request
grep "a1b2c3d4-e5f6-7890-abcd-ef1234567890" logs/*.log

# Trace request through microservices
Client → Service A (correlationId: abc-123)
         → Service B (correlationId: abc-123)
         → Service C (correlationId: abc-123)
```

### 4. Error Tracking
```javascript
try {
  const response = await fetch('/api/messages', {
    headers: {
      'x-correlation-id': 'my-custom-id-123'
    }
  });

  const result = await response.json();

  if (!result.success) {
    console.error(`Error ID: ${result.correlationId}`);
    // Report to error tracking service with correlationId
  }
} catch (error) {
  // correlationId helps track the exact request
}
```

## Client Usage

### Send correlationId (Optional)
```typescript
// Client can send their own correlationId
fetch('/api/conversations', {
  headers: {
    'x-user-id': userId,
    'x-correlation-id': 'client-generated-uuid'
  }
});
```

### Read correlationId from Response
```typescript
const response = await fetch('/api/conversations', {
  headers: { 'x-user-id': userId }
});

// Read from header
const correlationId = response.headers.get('X-Correlation-Id');

// Read from body
const result = await response.json();
console.log(result.correlationId); // Same value
```

## Complete Example

### Request Flow
```
1. Client sends request (optional correlationId)
   POST /api/conversations/123/messages
   Headers:
     x-user-id: user-456
     x-correlation-id: abc-123 (optional)

2. Middleware generates or uses correlationId
   req.headers['x-correlation-id'] = 'abc-123' or new UUID

3. Controller processes request
   const correlationId = req.headers['x-correlation-id'];

4a. Success Response
   {
     "success": true,
     "data": {...},
     "correlationId": "abc-123"
   }

4b. Error Response (if exception thrown)
   {
     "success": false,
     "error": {
       "code": "NotFoundError",
       "message": "Conversation not found"
     },
     "correlationId": "abc-123"
   }
```

### Response Headers
```
HTTP/1.1 200 OK
X-Correlation-Id: abc-123
Content-Type: application/json

{
  "success": true,
  "data": {...},
  "correlationId": "abc-123"
}
```

## Logging Example

```typescript
// In your logger
import logger from 'jet-logger';

export const logRequest = (req: Request) => {
  const correlationId = req.headers['x-correlation-id'];
  logger.info(`[${correlationId}] ${req.method} ${req.path}`);
};

export const logError = (correlationId: string, error: Error) => {
  logger.error(`[${correlationId}] Error:`, error.message);
};

// In controller
public async sendMessage(req: Request, res: Response) {
  const correlationId = req.headers['x-correlation-id'] as string;

  try {
    logRequest(req);
    // ... business logic ...
  } catch (error) {
    logError(correlationId, error);
    throw error;
  }
}
```

## Key Points

1. ✅ **Single Source**: correlationId generated once in middleware
2. ✅ **Consistent**: Same ID in success and error responses
3. ✅ **Traceable**: Can track request through entire system
4. ✅ **Client-friendly**: Client can provide their own ID
5. ✅ **Header + Body**: Available in both response header and body
6. ✅ **Production-ready**: Supports distributed tracing
