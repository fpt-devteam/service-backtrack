# Chat Service — Connection & Messaging Flow

## Architecture Overview

```
Client App
    │
    │  Firebase JWT (Authorization header)
    ▼
API Gateway (YARP :5000)
    │  validates JWT via Firebase Admin
    │  injects x-auth-id header
    ▼
Chat Service (Express + Socket.IO :3000)
    │
    ├── REST  → Express routes
    └── WS    → Socket.IO at path /hub
```

All traffic flows through the API Gateway. The Chat service never validates tokens itself — it only trusts the `x-auth-id` header injected by the gateway.

---

## Phase 1 — Create a Conversation (REST)

Before any real-time messaging, a conversation must exist.

**Direct (DM) conversation:**
```
POST /api/chat/conversations/direct
Authorization: Bearer <firebase-jwt>
Body: { partnerId: "<other-user-uid>" }

→ 201 { conversationId: "..." }
```

**Org / support conversation:**
```
POST /api/chat/conversations/organization
Authorization: Bearer <firebase-jwt>
Body: { orgId: "..." }

→ 201 { conversationId: "..." }
```

Gateway sequence:
```
Client ──POST /api/chat/conversations/direct──▶ Gateway
                                                  │ verifyIdToken()
                                                  │ x-auth-id: <uid>
                                               ──▶ Chat Service
                                                  │ insert conversation + participants (MongoDB)
Client ◀── 201 { conversationId } ────────────────┘
```

---

## Phase 2 — WebSocket Connection & Auth

Connect to the Socket.IO server through the gateway using either path:

| Path | Gateway transform |
|------|-------------------|
| `/chat/hub/**` | strips `/chat` prefix |
| `/api/chat/hub/**` | strips `/api/chat` prefix |

Both resolve to the Socket.IO server at path `/hub` on the Chat service.

**Client-side connection:**
```js
import { io } from 'socket.io-client';

const socket = io(GATEWAY_URL, {
  path: '/hub',
  extraHeaders: {
    Authorization: `Bearer ${firebaseJwt}`,
  },
});
```

**Gateway → Chat service:**
```
Client ── WS Upgrade (Bearer JWT) ──▶ Gateway
                                        │ forwards x-auth-id header
                                     ──▶ Chat Service /hub
                                        │ socketAuthMiddleware
                                        │   reads socket.handshake.headers['x-auth-id']
                                        │   sets socket.data.userId
                                        │
                                        │ socket auto-joins room: user:{userId}
                                        │ registerSocketHandlers(socket)
Client ◀── connect ─────────────────────┘
```

---

## Phase 3 — Join a Conversation Room

After connecting, emit `join:conversation` to subscribe to real-time events for a conversation.

```
Client ── emit("join:conversation", conversationId) ──▶ Chat Service
                                                          │ verify ConversationParticipant:
                                                          │   memberId == userId
                                                          │   isActive == true
                                                          │ socket.join("conversation:{id}")
Client ◀── emit("join:conversation:success", { conversationId }) ──┘

// On failure:
Client ◀── emit("join:conversation:error", { code: "FORBIDDEN" | "UNAUTHORIZED" })
```

---

## Phase 4 — Send a Message

**Direct message:**
```
Client ── emit("message:send", {
              conversationId: "...",
              content: "Hello!",
              type: "text",           // optional, defaults to text
              attachments: [...]      // optional
          }) ──▶ Chat Service
                  │ persist message to MongoDB
                  │ update conversation.lastMessage
                  │ increment unreadCount for all other participants
                  │
                  │── emit("message:new", message) ──────────▶ room: conversation:{id}
                  │── emit("conversation:updated", {           (all other participants)
                  │        unreadCount, lastMessage })
                  │         ──▶ each participant's user:{id} room
                  │
Client ◀── emit("message:send:success", { conversationId, message, isNewConversation })
Client ◀── emit("conversation:updated", { unreadCount: 0, lastMessage })
```

**Org / support message:**
```
Client ── emit("message:send:support", { conversationId, content, type? })
// Same broadcast behaviour as above
Client ◀── emit("message:send:support:success", { ... })
```

### Auto-join on first message

When `isNewConversation: true` is returned, the server automatically pulls all participants' open sockets into the conversation room — they do not need to call `join:conversation` manually.

---

## Phase 5 — Read Receipts

```
Client ── emit("conversation:read", { conversationId }) ──▶ Chat Service
                                                             │ reset unreadCount in MongoDB
                                                             │ markMessagesAsSeen()
                                                             │── emit("message:seen", {
                                                             │        conversationId,
                                                             │        readBy: userId,
                                                             │        readAt
                                                             │   }) ──▶ room (other participants)
Client ◀── emit("conversation:updated", { unreadCount: 0 })  (user:{id} room, syncs other tabs)
```

---

## Phase 6 — Typing Indicators

```
Client ── emit("typing:start", { conversationId, displayName? })
       ──▶ Chat Service ──▶ emit("typing:user", { userId, isTyping: true }) ──▶ room

Client ── emit("typing:stop",  { conversationId })
       ──▶ Chat Service ──▶ emit("typing:user", { userId, isTyping: false }) ──▶ room
```

---

## Complete Event Reference

### Client → Server

| Event | Payload | Description |
|-------|---------|-------------|
| `join:conversation` | `conversationId: string` | Subscribe to a conversation room |
| `leave:conversation` | `conversationId: string` | Unsubscribe from a conversation room |
| `message:send` | `{ conversationId, content, type?, attachments? }` | Send a direct message |
| `message:send:support` | `{ conversationId, content, type?, attachments? }` | Send an org/support message |
| `conversation:read` | `{ conversationId }` | Mark conversation as read |
| `typing:start` | `{ conversationId, displayName? }` | Notify typing started |
| `typing:stop` | `{ conversationId }` | Notify typing stopped |
| `join:org:queue` | `{ orgId, limit?, cursor? }` | Staff: subscribe to org support queue |
| `leave:org:queue` | `orgId: string` | Staff: unsubscribe from org queue |

### Server → Client

| Event | Payload | Target | Description |
|-------|---------|--------|-------------|
| `join:conversation:success` | `{ conversationId }` | sender | Room join confirmed |
| `join:conversation:error` | `{ code, message }` | sender | Room join denied |
| `message:new` | message object | room | New message from another participant |
| `message:send:success` | `{ conversationId, message, isNewConversation }` | sender | Send acknowledged |
| `message:send:error` | `{ code, message }` | sender | Send failed |
| `conversation:updated` | `{ conversationId, unreadCount, lastMessage }` | `user:{id}` room | Conversation list update (all tabs/devices) |
| `message:seen` | `{ conversationId, readBy, readAt }` | room | Another user read the conversation |
| `typing:user` | `{ conversationId, userId, displayName?, isTyping }` | room | Typing indicator |
| `conversation:new` | `{ conversationId, message }` | room | First message in a new conversation |
| `org:queue:list` | `{ orgId, ...paginated }` | sender | Staff queue snapshot |
| `org:queue:error` | `{ orgId, message }` | sender | Staff queue fetch failed |

---

## Room Naming Convention

| Room | Members | Purpose |
|------|---------|---------|
| `user:{userId}` | All sockets of one user | Push targeted events (unread count, conversation list updates) across all tabs/devices |
| `conversation:{conversationId}` | All participants currently in the conversation | Broadcast messages, typing, read receipts |
| `org:{orgId}:queue` | Staff sockets that joined the queue | Real-time support queue updates |
