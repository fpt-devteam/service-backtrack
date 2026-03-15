export interface IConversationQueue {
    id: string;
    conversationId: string;
    createdAt: Date;
    updatedAt: Date;
    deletedAt?: Date | null;
    takenAt?: Date | null;
    takenBy?: string | null;
}