import { Schema, model } from "mongoose";
import { IMessage, MessageStatus, MessageType } from "./interfaces/message.interface";

const MessageAttachmentSchema = new Schema({
	type: { type: String, enum: MessageType, required: true },
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
		conversationId: { type: String, required: true, index: true },
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

