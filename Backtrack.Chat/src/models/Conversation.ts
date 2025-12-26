import mongoose, { Document, Schema } from 'mongoose';
import { UserInfo } from './common/types';

export enum ConversationType {
  SINGLE = 'SINGLE',
  GROUP = 'GROUP',
}

export interface IConversation extends Document {
  type: ConversationType;
  name: string | null;
  createdBy: UserInfo;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
}

const ConversationSchema = new Schema<IConversation>(
  {
    type: {
      type: String,
      enum: Object.values(ConversationType),
      required: true,
    },
    name: {
      type: String,
      default: null,
    },
    createdBy: {
      id: { type: String, required: true },
      displayName: { type: String, required: true },
      avatarUrl: { type: String, default: null },
    },
    deletedAt: {
      type: Date,
      default: null,
    },
  },
  {
    timestamps: true,
  },
);

// Index for finding conversations by type
ConversationSchema.index({ type: 1, deletedAt: 1 });

// Index for finding conversations by creator
ConversationSchema.index({ 'createdBy.id': 1, updatedAt: -1 });

// Index for sorting by update time (used in getAllConversationsByUserId)
ConversationSchema.index({ updatedAt: -1, deletedAt: 1 });

const Conversation = mongoose.model<IConversation>(
  'Conversation',
  ConversationSchema,
);
export default Conversation;