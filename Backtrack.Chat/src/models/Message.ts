import { Schema, model, Document } from 'mongoose';

export enum MessageType {
    TEXT = 'text',
    IMAGE = 'image',
    FILE = 'file',
    }

export interface IMessage extends Document {
  conversationId: string;
  sender: {
    id: string,
    name: string,
    avatarUrl: string | null,
  };
  content: string;
  type: MessageType;
  replyToMessageId?: string | null;
  timestamp: Date;
  deletedAt?: Date | null;
}


const MessageSchema = new Schema<IMessage>({
  conversationId: { type: String, required: true, ref: 'Conversation' },
  sender: {
    id: { type: String, required: true },
    name: { type: String, required: true },
    avatarUrl: { type: String, default: null },
  },
  content: { type: String, required: true },
  type: { type: String, enum: Object.values(MessageType), required: true },
  replyToMessageId: { type: String, default: null },
  timestamp: { type: Date, default: Date.now },
  deletedAt: { type: Date, default: null },
});

// Existing index for basic message queries
MessageSchema.index({ conversationId: 1, timestamp: -1 });

// Index for cursor-based pagination with soft delete filtering
MessageSchema.index({ conversationId: 1, deletedAt: 1, timestamp: -1 });

// Index for finding messages by sender
MessageSchema.index({ 'sender.id': 1, timestamp: -1 });

const Message = model<IMessage>('Message', MessageSchema);

export default Message;