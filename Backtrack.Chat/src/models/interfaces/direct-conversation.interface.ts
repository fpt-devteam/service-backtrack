
export interface IDirectConversation {
    id: string;
	lastMessageAt: Date | null;
	lastMessageContent: string | null;
	senderId: string | null;
	createdAt: Date;
	updatedAt: Date;
	deletedAt: Date | null;
}