import mongoose, { Document, Schema } from 'mongoose';

export interface IConversation extends Document {
  lastMessageAt?: Date;      
  lastMessageContent?: string | null; 
  senderId?: string;
  customAvatarUrl?: string | null;
  createdAt: Date;
  updatedAt: Date;
  deletedAt?: Date | null;
}

const ConversationSchema = new Schema<IConversation>(
  {
    deletedAt: {
      type: Date,
      default: null,
    },
    senderId: { type: String, required: false, ref: 'User' },
    lastMessageAt: { type: Date, default: null },
    lastMessageContent: { type: String, default: null, maxlength: 200 },
    customAvatarUrl: { type: String, default: null },
  },
  {
    timestamps: true,
  },
);

ConversationSchema.index({ lastMessageAt: -1, deletedAt: 1 });
ConversationSchema.index({ updatedAt: -1, deletedAt: 1 });

const Conversation = mongoose.model<IConversation>(
  'Conversation',
  ConversationSchema,
);
export default Conversation;