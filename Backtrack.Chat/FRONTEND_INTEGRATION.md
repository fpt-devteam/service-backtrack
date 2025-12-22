# Frontend Integration Guide

## Overview
This document explains how the frontend has been updated to work with the new standardized API response format.

## Response Format Changes

### Before (Old Format)
```javascript
// Success - direct data
const conversations = await response.json();

// Error - variable format
const error = await response.json();
throw new Error(error.message);
```

### After (New Format)
```javascript
// Success
{
  "success": true,
  "data": [...],
  "correlationId": "uuid-string"
}

// Error
{
  "success": false,
  "error": {
    "code": "ErrorCode",
    "message": "Human readable message"
  },
  "correlationId": "uuid-string"
}
```

## Files Modified

### 1. `src/public/scripts/http.js`
**Purpose**: Centralized HTTP client with automatic response handling

**Changes**:
- Added `handleResponse()` function to unwrap API responses
- Automatically extracts `data` from successful responses
- Throws enhanced errors with `correlationId` for failed requests
- Returns clean data to calling code

**Usage**:
```javascript
// Automatically handles the new format
const data = await Http.get('/api/conversations');
// Returns: [...] (unwrapped data)

// Errors include correlationId
try {
  await Http.post('/api/conversations', body);
} catch (error) {
  console.error(error.message);
  console.error('Correlation ID:', error.correlationId);
}
```

### 2. `src/public/scripts/chat.js`
**Purpose**: Main chat application logic

**Changes Made**:

#### `loadConversations()`
```javascript
// Now checks result.success and extracts result.data
const result = await response.json();
if (!result.success) {
  throw new Error(result.error?.message);
}
const conversations = result.data || [];
```

#### `loadMessages()`
```javascript
// Backend returns: { success: true, data: { data: [...], pagination: {...} } }
const result = await response.json();
const messages = result.data?.data || [];

// Ensure messages is always an array
if (!Array.isArray(messages)) {
    state.messages = [];
} else {
    state.messages = messages;
}
```

#### `sendMessage()`
```javascript
// Validates result.success before proceeding
const result = await response.json();
if (!result.success) {
  throw new Error(result.error?.message);
}
```

#### `createConversation()`
```javascript
// Returns result.data instead of raw response
const result = await response.json();
return result.data;
```

#### `showError()`
```javascript
// Enhanced to display correlationId for debugging
function showError(message, correlationId) {
  let errorMessage = message;
  if (correlationId) {
    errorMessage += `\n\nCorrelation ID: ${correlationId}`;
  }
  alert(errorMessage);
}
```

### 3. `src/public/scripts/types.d.ts` (New File)
**Purpose**: TypeScript type definitions for API responses

**Includes**:
- `ApiResponse<T>` - Generic response wrapper
- `ApiError` - Error structure
- `Conversation`, `Message`, `User` - Domain models
- `PaginatedResponse<T>` - Pagination wrapper
- `ExtendedError` - Error with correlationId

## Error Handling

### Error Display
All errors now include the `correlationId` for easier debugging:

```javascript
try {
  await loadConversations();
} catch (error) {
  // Error message includes correlation ID
  showError(
    error.message || 'Không thể tải danh sách conversations',
    error.correlationId  // ✨ Now included
  );
}
```

### Console Logging
Errors are logged with their correlation IDs:

```javascript
console.error(`[${correlationId}] ${message}`);
```

## Migration Checklist

✅ Updated `http.js` to handle new response format
✅ Updated `loadConversations()` to extract `result.data`
✅ Updated `loadMessages()` to handle paginated responses
✅ Updated `sendMessage()` to check `result.success`
✅ Updated `createConversation()` to return `result.data`
✅ Enhanced error handling with `correlationId` tracking
✅ Created TypeScript type definitions

## Testing

### Test Success Responses
```javascript
// Should work with new format
await loadConversations();
await loadMessages(conversationId);
await sendMessage(conversationId, 'Hello');
await createConversation('SINGLE', [userId]);
```

### Test Error Responses
```javascript
// Should display error with correlation ID
try {
  await sendMessage(invalidId, '');
} catch (error) {
  // Alert will show: "Message content cannot be empty\n\nCorrelation ID: abc-123"
}
```

### Verify Console Logs
All errors should log in the format:
```
[correlation-id] Error message
```

## Benefits

1. **Consistent Error Handling**: All errors follow the same structure
2. **Better Debugging**: Correlation IDs help trace requests through logs
3. **Type Safety**: TypeScript definitions provide IDE support
4. **Cleaner Code**: Response unwrapping is centralized in `http.js`
5. **Future-Proof**: Easy to add more metadata (timestamps, versions, etc.)

## API Examples

### GET /api/conversations
**Success (200)**:
```json
{
  "success": true,
  "data": [
    {
      "_id": "conv-123",
      "name": "Chat Room",
      "type": "GROUP"
    }
  ],
  "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

**Error (400)**:
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

### GET /api/conversations/:id/messages
**Success (200)**:
```json
{
  "success": true,
  "data": {
    "data": [
      {
        "_id": "msg-456",
        "content": "Hello world",
        "sender": {
          "id": "user-789",
          "name": "John Doe"
        },
        "timestamp": "2025-12-21T12:00:00Z"
      }
    ],
    "pagination": {
      "nextCursor": "cursor-string",
      "hasMore": true
    }
  },
  "correlationId": "a8d3f2e1-4c5b-6d7e-8f9a-0b1c2d3e4f5a"
}
```

### POST /api/conversations/:id/messages
**Success (201)**:
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

## Backward Compatibility

The frontend code gracefully handles both formats:

```javascript
// Works with new format: { success: true, data: [...] }
// Also works with old format: [...] (direct array)
const messages = result.data?.items || result.data || [];
```

However, for optimal functionality, the backend should always use the new standardized format.

## Next Steps

1. ✅ Backend uses standardized response format
2. ✅ Frontend updated to handle new format
3. 🔄 Test all API endpoints with new format
4. 📝 Update API documentation
5. 🧪 Add integration tests

## Support

If you encounter errors, check:
1. The correlation ID in the error message
2. Browser console for detailed logs (includes correlation ID)
3. Backend logs matching the correlation ID
4. Network tab to see raw API responses

For questions, refer to `RESPONSE_FORMAT.md` for the complete API documentation.
