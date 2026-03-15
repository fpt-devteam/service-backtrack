export enum ConversationParticipantRole {
    CUSTOMER = 'customer',
    STAFF = 'staff',
}
export interface IConversationParticipant {
	id: string;
	conversationId: string;
	memberId: string | null;
    role: ConversationParticipantRole;
    isAssigee: boolean | null;
	nickName: string | null;

	lastReadAt: Date | null;
	lastReadMessageId: string | null;
	unreadCount: number | null;

	deletedAt: Date | null;
	createdAt: Date;
	updatedAt: Date;
}