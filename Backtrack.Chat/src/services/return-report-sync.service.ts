import { ReturnReportSyncEvent } from '@/events/return-report-event';
import { Handover } from '@/models/interfaces/direct-conversation.interface';
import DirectConversation from '@/models/direct-conversation';
import ConversationParticipant from '@/models/conversation-participant';
import { createDirectConvParticipants } from './conversation-paticipant.service';
import { toStringOrNull } from '@/utils/object-id';
import logger from '@/utils/logger';

/**
 * Find the DirectConversation shared by exactly these two users.
 * Returns the conversationId string, or null if none exists.
 */
const findDirectConversationId = async (userIdA: string, userIdB: string): Promise<string | null> => {
    const result = await ConversationParticipant.aggregate([
        { $match: { memberId: { $in: [userIdA, userIdB] }, deletedAt: null } },
        { $group: { _id: '$conversationId', count: { $sum: 1 } } },
        { $match: { count: 2 } },
        { $limit: 1 },
    ]);

    if (!result.length) return null;

    // Verify it is a DirectConversation (not a support conversation)
    const candidateId = result[0]._id as string;
    const direct = await DirectConversation.exists({ _id: candidateId, deletedAt: null });
    return direct ? candidateId : null;
};

/**
 * Find or create a DirectConversation between finderId and ownerId,
 * then upsert the `handover` field with the latest return report state.
 */
export const syncReturnReportHandover = async (event: ReturnReportSyncEvent): Promise<void> => {
    let conversationId = await findDirectConversationId(event.FinderId, event.OwnerId);

    if (!conversationId) {
        // No conversation yet — create one so the handover has somewhere to live
        const conversation = new DirectConversation();
        await conversation.save();
        conversationId = toStringOrNull(conversation._id);
        if (!conversationId) {
            logger.error('Failed to create DirectConversation for return report handover sync');
            return;
        }
        await createDirectConvParticipants(conversationId, event.OwnerId, event.FinderId);
        logger.info(`Created DirectConversation ${conversationId} for finder=${event.FinderId} owner=${event.OwnerId}`);
    }

    const handover: Handover = {
        id: event.C2CReturnReportId,
        finder: {
            id: event.FinderId,
            displayName: event.FinderDisplayName,
            avatarUrl: event.FinderAvatarUrl,
            email: event.FinderEmail,
            phone: null,
            showEmail: false,
            showPhone: false,
            globalRole: 'User',
            status: 'Active',
        },
        owner: {
            id: event.OwnerId,
            displayName: event.OwnerDisplayName,
            avatarUrl: event.OwnerAvatarUrl,
            email: event.OwnerEmail,
            phone: null,
            showEmail: false,
            showPhone: false,
            globalRole: 'User',
            status: 'Active',
        },
        finderPost: event.FinderPostId
            ? { id: event.FinderPostId, postType: event.FinderPostType ?? '', status: '', category: '', imageUrls: [], displayAddress: null, eventTime: '', createdAt: '' }
            : null,
        ownerPost: event.OwnerPostId
            ? { id: event.OwnerPostId, postType: event.OwnerPostType ?? '', status: '', category: '', imageUrls: [], displayAddress: null, eventTime: '', createdAt: '' }
            : null,
        status: event.Status,
        activatedByRole: event.ActivatedByRole,
        confirmedAt: event.ConfirmedAt,
        expiresAt: event.ExpiresAt,
        createdAt: event.CreatedAt,
    };

    await DirectConversation.findByIdAndUpdate(
        conversationId,
        { $set: { handover } },
        { new: true }
    ).exec();

    logger.info(`Handover synced for conversation ${conversationId} (returnReport=${event.C2CReturnReportId}, status=${event.Status})`);
};
