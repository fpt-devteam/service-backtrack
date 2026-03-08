import { Schema, model } from "mongoose";

export enum MessageType {
  TEXT = 'text',
  IMAGE = 'image',
  FILE = 'file',
  VIDEO = 'video',
}

export enum MessageStatus {
  SENT = 'sent',
  SEEN = 'seen',
  FAILED = 'failed',
}

export interface IMessageAttachment {
  type: 'image' | 'video' | 'file';
  url: string;
  fileName?: string;
  fileSize?: number;
  mimeType?: string;
  thumbnail?: string; //  video/image
  duration?: number; //video (seconds)
  width?: number;    // image/video
  height?: number;   // image/video
}

export interface IMessage {
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

const MessageAttachmentSchema = new Schema({
	type: { type: String, enum: ['image', 'video', 'file'], required: true },
	url: { type: String, required: true },
	fileName: { type: String },
	fileSize: { type: Number },
	mimeType: { type: String },
	thumbnail: { type: String },
	duration: { type: Number },
	width: { type: Number },
	height: { type: Number },
}, { _id: false });

const MessageSchema = new Schema<IMessage>(
	{
		conversationId: { type: Schema.Types.ObjectId, required: true, index: true },
		senderId: { type: String, required: true, index: true },
		type: { 
			type: String, 
			enum: Object.values(MessageType), 
			required: true,
			default: MessageType.TEXT 
		},
		content: { type: String, required: true },
		attachments: { type: [MessageAttachmentSchema], default: [] },
		deletedAt: { type: Date, default: null },
		status: { 
			type: String, 
			enum: Object.values(MessageStatus),
			default: MessageStatus.SENT 
		},
	},
	{ timestamps: true }
);

// Compound index for efficient pagination
MessageSchema.index({ conversationId: 1, createdAt: -1 });

const Message = model<IMessage>('Message', MessageSchema);
export default Message;

