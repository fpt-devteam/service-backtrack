export enum ConversationType {
	Direct = 'Direct',
	ORGANIZATION = 'organization',
}
export enum ConversationStatus {
	IN_QUEUE = 'queue',
	IN_PROGRESS = 'in_progress',
	CLOSED = 'closed',
}
export interface ISupportConversation {
    id: string;
	lastMessageAt: Date | null;
	lastMessageContent: string | null;
	senderId: string | null;
	staffAssignId: string | null;
	orgId: string;
	orgName: string | null;
	orgSlug: string | null;
	orgLogoUrl: string | null;
	status: ConversationStatus;
	createdAt: Date;
	updatedAt: Date;
	deletedAt: Date | null;
}