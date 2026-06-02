import SupportConversation from '@/models/support-conversation';
import { ConversationStatus } from '@/models/interfaces/support-conversation.interface';

export interface StaffChatStats {
    /** IN_PROGRESS + IN_VERIFIED assigned to this staff (sum of active claims) */
    activeChats: number;
    /** Claim requests waiting in the org queue (IN_QUEUE, unassigned) */
    queueWaiting: number;
    /** Claims currently being handled by this staff (IN_PROGRESS) */
    inProgress: number;
    /** Claims verified by this staff, awaiting resolution (IN_VERIFIED) */
    inVerified: number;
}

export const getStaffChatStats = async (staffId: string, orgId: string): Promise<StaffChatStats> => {
    const [inProgress, inVerified, queueWaiting] = await Promise.all([
        SupportConversation.countDocuments({
            staffAssignId: staffId,
            status: ConversationStatus.IN_PROGRESS,
            deletedAt: null,
        }),
        SupportConversation.countDocuments({
            staffAssignId: staffId,
            status: ConversationStatus.IN_VERIFIED,
            deletedAt: null,
        }),
        SupportConversation.countDocuments({
            orgId,
            status: ConversationStatus.IN_QUEUE,
            staffAssignId: null,
            lastMessageContent: { $ne: null },
            deletedAt: null,
        }),
    ]);

    return {
        activeChats: inProgress + inVerified,
        queueWaiting,
        inProgress,
        inVerified,
    };
};
