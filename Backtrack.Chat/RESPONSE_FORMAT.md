# API Response Format

## Overview
All API endpoints follow a consistent response format as defined in `src/response.json`.

## Response Structure

### Success Response
```json
{
  "success": true,
  "data": {
    // Your response data here
  },
  "correlationId": "uuid-string"
}
```

### Error Response
```json
{
  "success": false,
  "error": {
    "code": "ErrorCode",
    "message": "Human readable error message"
  },
  "correlationId": "uuid-string"
}
```

## Implementation

### Error Handling
The error handler (`src/middlewares/errorHandler.ts`) catches all exceptions and formats them:

```typescript
// Throws exception in service/controller
throw new BadRequestError('Message content cannot be empty');

// Error handler formats it automatically:
{
  "success": false,
  "error": {
    "code": "BadRequestError",
    "message": "Message content cannot be empty"
  },
  "correlationId": "a1b2c3..."
}
```

### Success Response
Controllers return success responses in the standard format:

```typescript
return res.status(HTTP_STATUS_CODES.Ok).json({
  success: true,
  data: result,
  correlationId: randomUUID(),
});
```

## Error Codes

### Standard Error Classes
- `BadRequestError` (400) - Invalid input or request
- `UnauthorizedError` (401) - Authentication required
- `ForbiddenError` (403) - Access denied
- `NotFoundError` (404) - Resource not found
- `ConflictError` (409) - Resource conflict
- `InternalServerError` (500) - Server error

### Usage Example
```typescript
// In service or controller
if (!user) {
  throw new NotFoundError('User not found');
}

// Results in:
{
  "success": false,
  "error": {
    "code": "NotFoundError",
    "message": "User not found"
  },
  "correlationId": "..."
}
```

## Correlation ID
Every response includes a unique `correlationId` (UUID v4) for:
- Request tracing
- Debugging
- Log aggregation
- Support tickets

## Examples

### GET /api/conversations
**Success (200):**
```json
{
  "success": true,
  "data": [
    {
      "_id": "conv-123",
      "name": "Chat Room",
      "type": "group"
    }
  ],
  "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

**Error (400):**
```json
{
  "success": false,
  "error": {
    "code": "BadRequestError",
    "message": "User ID is required in x-user-id header"
  },
  "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

### POST /api/conversations/:id/messages
**Success (201):**
```json
{
  "success": true,
  "data": {
    "_id": "msg-456",
    "content": "Hello world",
    "sender": {
      "id": "user-789",
      "name": "John Doe"
    },
    "timestamp": "2025-12-21T12:00:00Z"
  },
  "correlationId": "a8d3f2e1-4c5b-6d7e-8f9a-0b1c2d3e4f5a"
}
```

**Error (404):**
```json
{
  "success": false,
  "error": {
    "code": "NotFoundError",
    "message": "Conversation not found"
  },
  "correlationId": "a8d3f2e1-4c5b-6d7e-8f9a-0b1c2d3e4f5a"
}
```

## Client Integration

### TypeScript Client Example
```typescript
interface ApiResponse<T> {
  success: boolean;
  data?: T;
  error?: {
    code: string;
    message: string;
  };
  correlationId: string;
}

async function fetchConversations() {
  const response = await fetch('/api/conversations', {
    headers: {
      'x-user-id': userId
    }
  });

  const result: ApiResponse<Conversation[]> = await response.json();

  if (!result.success) {
    console.error(`Error [${result.correlationId}]:`, result.error);
    throw new Error(result.error.message);
  }

  return result.data;
}
```

### JavaScript Client Example
```javascript
fetch('/api/conversations', {
  headers: { 'x-user-id': userId }
})
  .then(res => res.json())
  .then(result => {
    if (!result.success) {
      console.error('Error:', result.error.message);
      console.error('Correlation ID:', result.correlationId);
      return;
    }

    console.log('Data:', result.data);
  });
```

## Notes

- All responses follow this format, including validation errors
- The `correlationId` is generated server-side for every request
- Error codes match the exception class names
- HTTP status codes are set appropriately (400, 404, 500, etc.)
- No error details/stack traces in production for security
