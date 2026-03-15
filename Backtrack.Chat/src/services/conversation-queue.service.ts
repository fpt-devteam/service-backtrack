import ConversationQueue from "@/models/conversation-queue";
import { CursorPaginationParams, cursorPaginate } from "@/utils/pagination";

export const createConversationQueue = async (
    conversationId: string,
) => {
    const conversationQueue = new ConversationQueue({
        conversationId,
    });
    await conversationQueue.save();
    return conversationQueue;
}

export const popConversationQueue = async (
    conversationId: string,
    staffId: string,
) => {
    const conversationQueue = await ConversationQueue.findOneAndUpdate(
        { conversationId },
        { takenBy: staffId, takenAt: new Date() },
        { new: true },
    );
    return conversationQueue;
}

export const pushConversationQueue = async (
    conversationId: string,
) => {
    const conversationQueue = await ConversationQueue.findOneAndUpdate(
        { conversationId },
        { takenBy: null, takenAt: null },
        { new: true },
    );
    return conversationQueue;
}

export const deleteConversationQueue = async (
    conversationId: string,
) => {
    const conversationQueue = await ConversationQueue.findOneAndUpdate(
        { conversationId },
        { deletedAt: new Date() },
        { new: true },
    );
    return conversationQueue;
}

/**
 * List conversations waiting in queue (not yet taken), sorted FIFO.
 * Oldest waiting conversation comes first (createdAt ascending).
 */
export const listConversationQueue = async (
    params: CursorPaginationParams = {},
) => {
    return cursorPaginate(
        ConversationQueue,
        { takenBy: null, deletedAt: null },
        params,
        'createdAt',
        1,
    );
};
