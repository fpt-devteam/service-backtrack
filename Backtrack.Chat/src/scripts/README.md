# Database Seeding Scripts

This directory contains scripts for seeding the Backtrack.Chat database with mock data for development and testing.

## Available Scripts

### 1. Seed Users Only
Seeds only user data into the database.

```bash
npm run seed:users
```

**What it does:**
- Clears existing users
- Creates 4 mock users (Alice, Bob, Charlie, Diana)
- User IDs match the frontend login options

**Mock Users:**
- Alice (`693d82480025a5c1d8f5b33e`) - alice@example.com
- Bob (`693d82480025a5c1d8f5b33f`) - bob@example.com
- Charlie (`693d82480025a5c1d8f5b340`) - charlie@example.com
- Diana (`693d82480025a5c1d8f5b341`) - diana@example.com

### 2. Seed All Data (Recommended)
Seeds a complete demo environment with users, conversations, and messages.

```bash
npm run seed:all
# or simply
npm run seed
```

**What it does:**
- Clears all existing data (users, conversations, participants, messages)
- Creates 4 mock users
- Creates 4 sample conversations:
  1. Alice ↔ Bob (Single chat)
  2. Alice ↔ Charlie (Single chat)
  3. Team Chat (Group with all 4 users)
  4. Bob ↔ Diana (Single chat)
- Adds conversation participants
- Creates sample messages in each conversation

**Data Created:**
- 👥 4 Users
- 💬 4 Conversations (3 single, 1 group)
- 👤 10 Participants
- 📨 13 Messages

## Prerequisites

1. **MongoDB Running**: Ensure MongoDB is accessible
2. **Environment Variables**: Set `MONGODB_CONNECTIONSTRING` in your env file
3. **Dependencies Installed**: Run `npm install`

## Usage Examples

### First Time Setup
```bash
# Install dependencies
npm install

# Seed all data for complete demo
npm run seed
```

### Reset Demo Data
```bash
# Clear and reseed everything
npm run seed:all
```

### Add Only Users
```bash
# If you only need users (useful if you already have conversations)
npm run seed:users
```

## Environment Configuration

The scripts use the connection string from your environment file:

**File**: `../env/backtrack-chat-api.docker.env`

```env
MONGODB_CONNECTIONSTRING=mongodb://localhost:27017/backtrack-chat
```

## Script Details

### seed-users.ts
Simple script that creates the 4 mock users.

**Output:**
```
Connecting to MongoDB...
✅ Connected to MongoDB
Clearing existing users...
✅ Cleared existing users
Inserting mock users...
✅ Successfully inserted 4 users

📋 Created Users:
────────────────────────────────────────────────────────────────────────────────
ID: 693d82480025a5c1d8f5b33e
Name: Alice
Email: alice@example.com
────────────────────────────────────────────────────────────────────────────────
...
```

### seed-all.ts
Comprehensive script that creates a complete demo environment.

**Output:**
```
Connecting to MongoDB...
✅ Connected to MongoDB
Clearing existing data...
✅ Cleared existing data
Inserting users...
✅ Inserted 4 users
Creating conversations...
✅ Created 4 conversations
Adding conversation participants...
✅ Added 10 participants
Adding messages...
✅ Added 13 messages

📊 Seeding Summary:
════════════════════════════════════════════════════════════════════════════════
👥 Users: 4
💬 Conversations: 4
👤 Participants: 10
📨 Messages: 13
════════════════════════════════════════════════════════════════════════════════

📋 Conversations Created:
────────────────────────────────────────────────────────────────────────────────
1. Alice ↔ Bob (Single) - 4 messages
2. Alice ↔ Charlie (Single) - 2 messages
3. Team Chat (Group) - 5 messages
4. Bob ↔ Diana (Single) - 2 messages
────────────────────────────────────────────────────────────────────────────────
```

## Sample Data

### Conversations

**1. Alice ↔ Bob (Single Chat)**
- Latest message: "Sure! 3pm works for me?" (Bob)
- 4 messages total
- Topics: Coffee plans

**2. Alice ↔ Charlie (Single Chat)**
- Latest message: "Yes! Just sent it to you via email" (Charlie)
- 2 messages total
- Topics: Work reports

**3. Team Chat (Group)**
- All 4 users participate
- Latest message: "Perfect! Let us start in 5 minutes" (Alice)
- 5 messages total
- Topics: Sprint planning meeting

**4. Bob ↔ Diana (Single Chat)**
- Latest message: "Sure! Will do it this afternoon" (Diana)
- 2 messages total
- Topics: Code review

## Troubleshooting

### Connection Failed
```
Error: MONGODB_CONNECTIONSTRING is not defined
```
**Solution**: Check your environment file has the MongoDB connection string.

### Database Not Accessible
```
❌ Database connection failed
```
**Solution**:
- Ensure MongoDB is running
- Check IP whitelist (for MongoDB Atlas)
- Verify connection string format

### TypeScript Errors
```
Cannot find module '@src/...'
```
**Solution**: Ensure `tsconfig-paths/register` is working:
```bash
npm install -D tsconfig-paths
```

## Adding More Seed Data

### To add more users:
Edit `seed-users.ts` or `seed-all.ts` and add to the `mockUsers` array:

```typescript
{
  _id: 'new-user-id-here',
  email: 'newuser@example.com',
  displayName: 'New User',
  avatarUrl: null,
  createdAt: new Date(),
  updatedAt: new Date(),
  deletedAt: null,
  syncedAt: new Date(),
}
```

### To add more conversations:
Edit `seed-all.ts` and create new conversations in the "Create Conversations" section:

```typescript
const conv5 = await Conversation.create({
  type: ConversationType.SINGLE,
  name: 'New Chat',
  createdBy: {
    id: 'creator-user-id',
    displayName: 'Creator Name',
    avatarUrl: null,
  },
});
```

## Notes

- **User IDs must match** between frontend and backend for login to work
- **Seeding clears existing data** - use with caution in production
- **Timestamps are realistic** - messages span over several days
- **Message content is casual** - demonstrates real-world usage

## Integration with Frontend

The seeded user IDs match the hardcoded users in `Backtrack.Chat.Web/src/scripts/login.js`:

```javascript
const availableUsers = [
  { username: 'Alice', id: '693d82480025a5c1d8f5b33e' },
  { username: 'Bob', id: '693d82480025a5c1d8f5b33f' },
  { username: 'Charlie', id: '693d82480025a5c1d8f5b340' },
  { username: 'Diana', id: '693d82480025a5c1d8f5b341' },
];
```

This ensures seamless integration between frontend and backend!
