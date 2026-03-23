export interface ISupportConversationAssignment {
    id: string;
    conversationId: string;
    agentId: string;
    assignedAt: Date;
    assignedBy: string | null;
    unassignedAt: Date | null;
    note: string | null;
    createdAt: Date;
    updatedAt: Date;
    deletedAt: Date | null;
}