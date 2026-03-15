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
    id: string;
	lastMessageAt: Date | null;
	lastMessageContent: string | null;
	senderId: string | null;
	type: ConversationType;
	staffAssignId: string | null;
	orgId: string | null;
	createdAt: Date;
	updatedAt: Date;
	deletedAt: Date | null;
}