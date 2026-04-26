import SupportConversation from '@/models/support-conversation';
import { ConversationStatus } from '@/models/interfaces/support-conversation.interface';

export interface StaffChatStats {
    activeChats: number;
    queueWaiting: number;
}

export const getStaffChatStats = async (staffId: string, orgId: string): Promise<StaffChatStats> => {
    const [activeChats, queueWaiting] = await Promise.all([
        SupportConversation.countDocuments({
            staffAssignId: staffId,
            status: ConversationStatus.IN_PROGRESS,
            deletedAt: null,
        }),
        SupportConversation.countDocuments({
            orgId,
            status: ConversationStatus.IN_QUEUE,
            staffAssignId: null,
            deletedAt: null,
        }),
    ]);

    return { activeChats, queueWaiting };
};
