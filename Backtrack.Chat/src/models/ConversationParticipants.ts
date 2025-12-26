// collection with id, conversationId, userId, role (admin/member), joinedAt
import { Schema, model, Document } from 'mongoose';
import { UserInfo } from './common/types';

export enum ParticipantRole {
  ADMIN = 'ADMIN',
  MEMBER = 'MEMBER',
}

export interface IConversationParticipant extends Document {
  conversationId: string;
  user: UserInfo;
  role: ParticipantRole;
  joinedAt: Date;
  deletedAt?: Date | null;
}

const ConversationParticipantSchema = new Schema<IConversationParticipant>({
  conversationId: { type: String, required: true, ref: 'Conversation' },
  user: {
    id: { type: String, required: true },
    displayName: { type: String, required: true },
    avatarUrl: { type: String, default: null },
  },
  role: { type: String, enum: Object.values(ParticipantRole), required: true },
  joinedAt: { type: Date, default: Date.now },
  deletedAt: { type: Date, default: null },
});

// CRITICAL INDEX: For getAllConversationsByUserId() - finds all user's conversations
ConversationParticipantSchema.index({ 'user.id': 1, deletedAt: 1 });

// CRITICAL INDEX: For participant existence checks on EVERY message send/receive
ConversationParticipantSchema.index({
  conversationId: 1,
  'user.id': 1,
  deletedAt: 1,
});

// CRITICAL INDEX: For finding all participants in a conversation
ConversationParticipantSchema.index({ conversationId: 1, deletedAt: 1 });

const ConversationParticipant = model<IConversationParticipant>(
  'ConversationParticipant',
  ConversationParticipantSchema,
);

export default ConversationParticipant;