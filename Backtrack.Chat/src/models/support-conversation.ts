import { Schema, model } from 'mongoose';
import { ConversationStatus, ISupportConversation } from './interfaces/support-conversation.interface';



const ConversationSchema = new Schema<ISupportConversation>(
	{
		lastMessageAt: { type: Date },
		lastMessageContent: { type: String, default: null },
		senderId: { type: String, default: null }, 
		staffAssignId: { type: String, default: null, index: true },
		orgId: { 
			type: String, 
			required: true,
			index: true 
		},
		status: { 
			type: String, 
			enum: Object.values(ConversationStatus), 
			required: true,
			index: true 
		},
		deletedAt: { type: Date, default: null }, 
	},
	{ 
		timestamps: true,
		minimize: false,
	},
);

ConversationSchema.index({ orgId: 1, lastMessageAt: -1 }); 
ConversationSchema.index({ senderId: 1, lastMessageAt: -1 }); 
ConversationSchema.index({ deletedAt: 1 }); 
ConversationSchema.index({ staffAssignId: 1, lastMessageAt: -1 });

const SupportConversation = model<ISupportConversation>('SupportConversation', ConversationSchema);
export default SupportConversation;