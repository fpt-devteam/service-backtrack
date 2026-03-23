import ConversationAssignment from "@/models/support-conversation-assignment";
import { CursorPaginationParams, cursorPaginate } from "@/utils/pagination";

export const assignConversation = async (
    conversationId: string,
    staffId: string,
) => {
    const assignment = new ConversationAssignment({
        conversationId,
        agentId: staffId,
        assignedAt: new Date(),
        assignedBy: null,
    });
    await assignment.save();
    return assignment;
};

export const unassignConversation = async (
    conversationId: string,
    staffId: string,
) => {
    const assignment = await ConversationAssignment.findOneAndUpdate(
        { conversationId, agentId: staffId, unassignedAt: null },
        { unassignedAt: new Date() },
        { new: true },
    );
    return assignment;
};

export const getActiveAssignment = async (
    conversationId: string,
) => {
    const assignment = await ConversationAssignment.findOne(
        { conversationId, unassignedAt: null, deletedAt: null },
    ).lean().exec();
    return assignment;
};

/**
 * List active assignments for a specific agent with cursor-based pagination.
 * Sorted by assignedAt descending (most recent first).
 */
export const listAssignmentsByAgent = async (
    agentId: string,
    params: CursorPaginationParams = {},
) => {
    return cursorPaginate(
        ConversationAssignment,
        { agentId, unassignedAt: null, deletedAt: null },
        params,
        'assignedAt',
        -1,
    );
};

/**
 * List all assignments for a conversation (full history) with cursor-based pagination.
 * Sorted by assignedAt descending.
 */
export const listAssignmentsByConversation = async (
    conversationId: string,
    params: CursorPaginationParams = {},
) => {
    return cursorPaginate(
        ConversationAssignment,
        { conversationId, deletedAt: null },
        params,
        'assignedAt',
        -1,
    );
};
