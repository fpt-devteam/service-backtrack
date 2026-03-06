import { Schema, model } from 'mongoose';

export enum ConversationType {
	PERSONAL = 'personal',
	ORGANIZATION = 'organization',
}

export enum TicketStatus {
	QUEUED = 'queued',
	ASSIGNED = 'assigned',
	RESOLVED = 'resolved',
	ESCALATED = 'escalated',
}

export interface IConversation {
	lastMessageAt?: Date;
	lastMessageContent?: string | null;
	senderId?: string;
	type: ConversationType;
	orgId?: string | null;
	ticketStatus?: TicketStatus;
	createdAt: Date;
	updatedAt: Date;
	deletedAt?: Date | null;
}

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
		orgId: { 
			type: String, 
			default: null,
			index: true 
		},
		ticketStatus: {
			type: String,
			enum: Object.values(TicketStatus),
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

ConversationSchema.index({ orgId: 1, ticketStatus: 1 });
ConversationSchema.index({ orgId: 1, lastMessageAt: -1 }); 
ConversationSchema.index({ senderId: 1, lastMessageAt: -1 }); 
ConversationSchema.index({ deletedAt: 1 }); 

const Conversation = model<IConversation>('Conversation', ConversationSchema);
export default Conversation;