/**
 * Seed All Data Script
 *
 * This script seeds the database with:
 * - Mock users
 * - Sample conversations
 * - Conversation participants
 * - Sample messages
 *
 * Run with: npm run seed:all or npm run seed
 */

import '../configs/env-loader';
import mongoose from 'mongoose';
import { User } from '@src/models/user.model';
import Conversation from '@src/models/conversation.model';
import ConversationParticipant from '@src/models/conversation-participant.model';
import Message, { MessageType } from '@src/models/message.model';
import ENV from '@src/common/constants/ENV';
import logger from '@src/utils/logger';

// Mock users data - matching frontend login options
const mockUsers = [
  {
    _id: '693d82480025a5c1d8f5b33e',
    email: 'alice@example.com',
    displayName: 'Alice',
    avatarUrl: null,
    createdAt: new Date('2024-12-10T10:00:00Z'),
    updatedAt: new Date('2024-12-10T10:00:00Z'),
    deletedAt: null,
    syncedAt: new Date(),
  },
  {
    _id: '693d82480025a5c1d8f5b33f',
    email: 'bob@example.com',
    displayName: 'Bob',
    avatarUrl: null,
    createdAt: new Date('2024-12-10T10:00:00Z'),
    updatedAt: new Date('2024-12-10T10:00:00Z'),
    deletedAt: null,
    syncedAt: new Date(),
  },
  {
    _id: '693d82480025a5c1d8f5b340',
    email: 'charlie@example.com',
    displayName: 'Charlie',
    avatarUrl: null,
    createdAt: new Date('2024-12-10T10:00:00Z'),
    updatedAt: new Date('2024-12-10T10:00:00Z'),
    deletedAt: null,
    syncedAt: new Date(),
  },
  {
    _id: '693d82480025a5c1d8f5b341',
    email: 'diana@example.com',
    displayName: 'Diana',
    avatarUrl: null,
    createdAt: new Date('2024-12-10T10:00:00Z'),
    updatedAt: new Date('2024-12-10T10:00:00Z'),
    deletedAt: null,
    syncedAt: new Date(),
  },
];

async function seedAll() {
  try {
    // Connect to database
    logger.info('🔌 Connecting to MongoDB...');
    const connectionString = ENV.MongodbConnectionstring;

    if (!connectionString) {
      throw new Error('MONGODB_CONNECTIONSTRING is not defined');
    }

    await mongoose.connect(connectionString);
    logger.info('✅ Connected to MongoDB successfully');

    // Clear existing data
    logger.info('🗑️  Clearing existing data...');
    await Promise.all([
      User.deleteMany({}),
      Conversation.deleteMany({}),
      ConversationParticipant.deleteMany({}),
      Message.deleteMany({}),
    ]);
    logger.info('✅ Cleared all existing data');

    // 1. Insert Users
    logger.info('👥 Inserting users...');
    const users = await User.insertMany(mockUsers);
    logger.info(`✅ Successfully inserted ${users.length} users`);

    // 2. Create Conversations (simplified schema - no type/name/createdBy)
    logger.info('💬 Creating conversations...');

    // Conversation 1: Alice & Bob (Single)
    const conv1 = await Conversation.create({
      createdAt: new Date('2024-12-20T10:00:00Z'),
      updatedAt: new Date('2024-12-25T15:30:00Z'),
    });

    // Conversation 2: Alice & Charlie (Single)
    const conv2 = await Conversation.create({
      createdAt: new Date('2024-12-22T14:00:00Z'),
      updatedAt: new Date('2024-12-24T18:20:00Z'),
    });

    // Conversation 3: Team Group (Alice, Bob, Charlie, Diana)
    const conv3 = await Conversation.create({
      createdAt: new Date('2024-12-15T09:00:00Z'),
      updatedAt: new Date('2024-12-25T12:45:00Z'),
    });

    // Conversation 4: Bob & Diana (Single)
    const conv4 = await Conversation.create({
      createdAt: new Date('2024-12-18T16:00:00Z'),
      updatedAt: new Date('2024-12-23T11:10:00Z'),
    });

    logger.info('✅ Created 4 conversations');

    // 3. Add Conversation Participants (simplified - memberId only)
    logger.info('👤 Adding conversation participants...');

    const participants = [
      // Conversation 1: Alice & Bob
      {
        conversationId: conv1._id.toString(),
        memberId: '693d82480025a5c1d8f5b33e',
      },
      {
        conversationId: conv1._id.toString(),
        memberId: '693d82480025a5c1d8f5b33f',
      },

      // Conversation 2: Alice & Charlie
      {
        conversationId: conv2._id.toString(),
        memberId: '693d82480025a5c1d8f5b33e',
      },
      {
        conversationId: conv2._id.toString(),
        memberId: '693d82480025a5c1d8f5b340',
      },

      // Conversation 3: Team Group (All 4 users)
      {
        conversationId: conv3._id.toString(),
        memberId: '693d82480025a5c1d8f5b33e',
      },
      {
        conversationId: conv3._id.toString(),
        memberId: '693d82480025a5c1d8f5b33f',
      },
      {
        conversationId: conv3._id.toString(),
        memberId: '693d82480025a5c1d8f5b340',
      },
      {
        conversationId: conv3._id.toString(),
        memberId: '693d82480025a5c1d8f5b341',
      },

      // Conversation 4: Bob & Diana
      {
        conversationId: conv4._id.toString(),
        memberId: '693d82480025a5c1d8f5b33f',
      },
      {
        conversationId: conv4._id.toString(),
        memberId: '693d82480025a5c1d8f5b341',
      },
    ];

    await ConversationParticipant.insertMany(participants);
    logger.info(`✅ Added ${participants.length} participants`);

    // 4. Add Messages (simplified - senderId only)
    logger.info('📨 Adding messages...');

    const messages = [
      // Conversation 1: Alice & Bob
      {
        conversationId: conv1._id.toString(),
        senderId: '693d82480025a5c1d8f5b33e',
        content: 'Hey Bob! How are you?',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv1._id.toString(),
        senderId: '693d82480025a5c1d8f5b33f',
        content: 'Hi Alice! I am doing great, thanks for asking!',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv1._id.toString(),
        senderId: '693d82480025a5c1d8f5b33e',
        content: 'That is awesome! Want to grab coffee later?',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv1._id.toString(),
        senderId: '693d82480025a5c1d8f5b33f',
        content: 'Sure! 3pm works for me?',
        type: MessageType.TEXT,
      },

      // Conversation 2: Alice & Charlie
      {
        conversationId: conv2._id.toString(),
        senderId: '693d82480025a5c1d8f5b33e',
        content: 'Charlie, did you finish the report?',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv2._id.toString(),
        senderId: '693d82480025a5c1d8f5b340',
        content: 'Yes! Just sent it to you via email',
        type: MessageType.TEXT,
      },

      // Conversation 3: Team Group
      {
        conversationId: conv3._id.toString(),
        senderId: '693d82480025a5c1d8f5b33e',
        content: 'Good morning team! Ready for our sprint planning?',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv3._id.toString(),
        senderId: '693d82480025a5c1d8f5b33f',
        content: 'Yes, ready!',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv3._id.toString(),
        senderId: '693d82480025a5c1d8f5b340',
        content: 'Let me grab my coffee first ☕',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv3._id.toString(),
        senderId: '693d82480025a5c1d8f5b341',
        content: 'I have prepared the backlog items for review',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv3._id.toString(),
        senderId: '693d82480025a5c1d8f5b33e',
        content: 'Perfect! Let us start in 5 minutes',
        type: MessageType.TEXT,
      },

      // Conversation 4: Bob & Diana
      {
        conversationId: conv4._id.toString(),
        senderId: '693d82480025a5c1d8f5b33f',
        content: 'Diana, can you review my pull request?',
        type: MessageType.TEXT,
      },
      {
        conversationId: conv4._id.toString(),
        senderId: '693d82480025a5c1d8f5b341',
        content: 'Sure! Will do it this afternoon',
        type: MessageType.TEXT,
      },
    ];

    await Message.insertMany(messages);
    logger.info(`✅ Added ${messages.length} messages`);


  } catch (error) {
    logger.error('❌ Error seeding data:');
    logger.error(error instanceof Error ? error.message : String(error));
    if (error instanceof Error && error.stack) {
      logger.error(error.stack);
    }
    // eslint-disable-next-line n/no-process-exit
    process.exit(1);
  } finally {
    // Disconnect from database
    await mongoose.disconnect();
    logger.info('👋 Disconnected from MongoDB');
    // eslint-disable-next-line n/no-process-exit
    process.exit(0);
  }
}

// Run the seed function
seedAll();
