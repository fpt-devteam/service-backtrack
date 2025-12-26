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

import '../config/env-loader';
import mongoose from 'mongoose';
import { User } from '@src/models/User';
import Conversation, { ConversationType } from '@src/models/Conversation';
import ConversationParticipant, {
  ParticipantRole,
} from '@src/models/ConversationParticipants';
import Message, { MessageType } from '@src/models/Message';
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

    // 2. Create Conversations
    logger.info('💬 Creating conversations...');

    // Conversation 1: Alice & Bob (Single)
    const conv1 = await Conversation.create({
      type: ConversationType.SINGLE,
      name: 'Bob',
      createdBy: {
        id: '693d82480025a5c1d8f5b33e',
        displayName: 'Alice',
        avatarUrl: null,
      },
      createdAt: new Date('2024-12-20T10:00:00Z'),
      updatedAt: new Date('2024-12-25T15:30:00Z'),
    });

    // Conversation 2: Alice & Charlie (Single)
    const conv2 = await Conversation.create({
      type: ConversationType.SINGLE,
      name: 'Charlie',
      createdBy: {
        id: '693d82480025a5c1d8f5b33e',
        displayName: 'Alice',
        avatarUrl: null,
      },
      createdAt: new Date('2024-12-22T14:00:00Z'),
      updatedAt: new Date('2024-12-24T18:20:00Z'),
    });

    // Conversation 3: Team Group (Alice, Bob, Charlie, Diana)
    const conv3 = await Conversation.create({
      type: ConversationType.GROUP,
      name: 'Team Chat',
      createdBy: {
        id: '693d82480025a5c1d8f5b33e',
        displayName: 'Alice',
        avatarUrl: null,
      },
      createdAt: new Date('2024-12-15T09:00:00Z'),
      updatedAt: new Date('2024-12-25T12:45:00Z'),
    });

    // Conversation 4: Bob & Diana (Single)
    const conv4 = await Conversation.create({
      type: ConversationType.SINGLE,
      name: 'Diana',
      createdBy: {
        id: '693d82480025a5c1d8f5b33f',
        displayName: 'Bob',
        avatarUrl: null,
      },
      createdAt: new Date('2024-12-18T16:00:00Z'),
      updatedAt: new Date('2024-12-23T11:10:00Z'),
    });

    logger.info('✅ Created 4 conversations');

    // 3. Add Conversation Participants
    logger.info('👤 Adding conversation participants...');

    const participants = [
      // Conversation 1: Alice & Bob
      {
        conversationId: conv1._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        role: ParticipantRole.ADMIN,
        joinedAt: new Date('2024-12-20T10:00:00Z'),
      },
      {
        conversationId: conv1._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-20T10:00:00Z'),
      },

      // Conversation 2: Alice & Charlie
      {
        conversationId: conv2._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        role: ParticipantRole.ADMIN,
        joinedAt: new Date('2024-12-22T14:00:00Z'),
      },
      {
        conversationId: conv2._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b340',
          displayName: 'Charlie',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-22T14:00:00Z'),
      },

      // Conversation 3: Team Group (All 4 users)
      {
        conversationId: conv3._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        role: ParticipantRole.ADMIN,
        joinedAt: new Date('2024-12-15T09:00:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-15T09:00:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b340',
          displayName: 'Charlie',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-15T09:00:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b341',
          displayName: 'Diana',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-15T09:00:00Z'),
      },

      // Conversation 4: Bob & Diana
      {
        conversationId: conv4._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        role: ParticipantRole.ADMIN,
        joinedAt: new Date('2024-12-18T16:00:00Z'),
      },
      {
        conversationId: conv4._id.toString(),
        user: {
          id: '693d82480025a5c1d8f5b341',
          displayName: 'Diana',
          avatarUrl: null,
        },
        role: ParticipantRole.MEMBER,
        joinedAt: new Date('2024-12-18T16:00:00Z'),
      },
    ];

    await ConversationParticipant.insertMany(participants);
    logger.info(`✅ Added ${participants.length} participants`);

    // 4. Add Messages
    logger.info('📨 Adding messages...');

    const messages = [
      // Conversation 1: Alice & Bob
      {
        conversationId: conv1._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        content: 'Hey Bob! How are you?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T15:00:00Z'),
      },
      {
        conversationId: conv1._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        content: 'Hi Alice! I am doing great, thanks for asking!',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T15:05:00Z'),
      },
      {
        conversationId: conv1._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        content: 'That is awesome! Want to grab coffee later?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T15:10:00Z'),
      },
      {
        conversationId: conv1._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        content: 'Sure! 3pm works for me?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T15:30:00Z'),
      },

      // Conversation 2: Alice & Charlie
      {
        conversationId: conv2._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        content: 'Charlie, did you finish the report?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-24T17:00:00Z'),
      },
      {
        conversationId: conv2._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b340',
          displayName: 'Charlie',
          avatarUrl: null,
        },
        content: 'Yes! Just sent it to you via email',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-24T18:20:00Z'),
      },

      // Conversation 3: Team Group
      {
        conversationId: conv3._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        content: 'Good morning team! Ready for our sprint planning?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T09:00:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        content: 'Yes, ready!',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T09:05:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b340',
          displayName: 'Charlie',
          avatarUrl: null,
        },
        content: 'Let me grab my coffee first ☕',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T09:07:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b341',
          displayName: 'Diana',
          avatarUrl: null,
        },
        content: 'I have prepared the backlog items for review',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T09:10:00Z'),
      },
      {
        conversationId: conv3._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33e',
          displayName: 'Alice',
          avatarUrl: null,
        },
        content: 'Perfect! Let us start in 5 minutes',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-25T12:45:00Z'),
      },

      // Conversation 4: Bob & Diana
      {
        conversationId: conv4._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b33f',
          displayName: 'Bob',
          avatarUrl: null,
        },
        content: 'Diana, can you review my pull request?',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-23T10:00:00Z'),
      },
      {
        conversationId: conv4._id.toString(),
        sender: {
          id: '693d82480025a5c1d8f5b341',
          displayName: 'Diana',
          avatarUrl: null,
        },
        content: 'Sure! Will do it this afternoon',
        type: MessageType.TEXT,
        timestamp: new Date('2024-12-23T11:10:00Z'),
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
