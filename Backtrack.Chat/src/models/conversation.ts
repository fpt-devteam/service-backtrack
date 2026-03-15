import { Schema, model } from 'mongoose';
import { ConversationType, IConversation } from './interfaces/conversation.interface';



const ConversationSchema = new Schema<IConversation>(
	{
		lastMessageAt: { type: Date },
		lastMessageContent: { type: String, default: null },
		senderId: { type: String, default: null }, 
		type: { 
			type: String, 
			enum: Object.values(ConversationType), 
			required: true,
			index: true 
		},
		staffAssignId: { type: String, default: null, index: true },
		orgId: { 
			type: String, 
			default: null,
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

const Conversation = model<IConversation>('Conversation', ConversationSchema);
export default Conversation;