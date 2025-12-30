import { Schema, model, Document } from 'mongoose';

export enum MessageType {
  TEXT = 'text',
  IMAGE = 'image',
  FILE = 'file',
  VIDEO = 'video',
}

export enum MessageStatus {
  SENDING = 'sending',
  SENT = 'sent',
  FAILED = 'failed',
}
export interface IMessageAttachment {
  type: 'image' | 'video' | 'file';
  url: string;
  fileName?: string;
  fileSize?: number;
  mimeType?: string;
  thumbnail?: string; // Cho video/image
  duration?: number; //video (seconds)
  width?: number;    // Cho image/video
  height?: number;   // Cho image/video
}
export interface IMessage extends Document {
  conversationId: Schema.Types.ObjectId | string;
  senderId: string;

  type: MessageType;
  content: string;
  attachments?: IMessageAttachment[];

  deletedAt?: Date | null;
  status?: MessageStatus;

  createdAt: Date;
  updatedAt: Date;
}

const MessageSchema = new Schema<IMessage>({
  conversationId: {
    type: Schema.Types.ObjectId,
    required: true,
    ref: 'Conversation'
  },
  senderId: { type: String, required: true, ref: 'User' },

  type: { type: String, enum: Object.values(MessageType), required: true },
  content: { type: String, default: null, required: true },
  attachments: {
    type: [
      {
        type: { type: String, required: true },
        url: { type: String, required: true },
        fileName: String,
        fileSize: Number,
        thumbnail: String,
        duration: Number,
        width: Number,
        height: Number,
      },
    ],
    default: [],
  },
  deletedAt: { type: Date, default: null },
  status: {
    type: String,
    enum: Object.values(MessageStatus),
    default: MessageStatus.SENT,
  },
}
  ,
  { timestamps: true },
);

// Index for cursor-based pagination with soft delete filtering
MessageSchema.index({ conversationId: 1, deletedAt: 1, updatedAt: -1 });

// Index for finding messages by sender
MessageSchema.index({ 'senderId': 1, updatedAt: -1 });

MessageSchema.index({ replyToMessageId: 1 });

const Message = model<IMessage>('Message', MessageSchema);

export default Message;