import mongoose from 'mongoose';
import { CreateConversationRequest } from "@/dtos/conversation/conversation.request";
import Conversation, { ConversationType, IConversation, TicketStatus } from "@/models/conversation";
import ConversationParticipant, { ConversationParticipantRole } from "@/models/conversation-participant";
import { ConversationErrors } from "./errors/conversation.errors";
import { ConversationPartner, ConversationResponse, ConversationsListResult } from "@/dtos/conversation/conversation.response";
import User from "@/models/user.model";
import { Constants } from '@/config/constants';
import { buildPaginatedResult, CursorPaginationParams } from '@/utils/pagination';
import { toObjectIdOrNull } from '@/utils/object-id';
import { createConvParticipants } from './conversation-paticipant.service';

export const createConversation = async (data: CreateConversationRequest, userId: string): Promise<IConversation> => {
  if (data.type === ConversationType.PERSONAL) {
    const duplicate = await ConversationParticipant.aggregate([
      { $match: { memberId: { $in: [data.memberId, userId] } } },
      { $group: { _id: '$conversationId', count: { $sum: 1 } } },
      { $match: { count: 2 } },
      { $limit: 1 },
    ]);

    if (duplicate.length > 0) {
      throw ConversationErrors.AlreadyExists;
    }
  }

  const conversation = new Conversation({
    type: data.type,
    ...(data.type === ConversationType.ORGANIZATION && {
      orgId: data.orgId,
      ticketStatus: data.ticketStatus,
    }),
  });
  await conversation.save();
  await createConvParticipants(conversation._id, data, userId);
  return conversation;
};



export const getConversationById = async (id: string, userId: string): Promise<ConversationResponse | null> => {
    const objectId = toObjectIdOrNull(id);
    const conversation = await Conversation.findById(objectId).lean().exec();
    
    if (!conversation || conversation.deletedAt) {
        return null;
    }

    const participant = await ConversationParticipant.findOne({
        conversationId: objectId,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    let partner: ConversationPartner | null = null;

    // Get the other participant (partner) for both personal and organization conversations
    const otherParticipant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: { $ne: userId },
        deletedAt: null
    }).exec();

    if (otherParticipant) {
        const partnerUser = await User.findById(otherParticipant.memberId)
            .select('displayName email avatarUrl')
            .lean()
            .exec();

        if (partnerUser) {
            partner = {
                id: partnerUser._id.toString(),
                displayName: partnerUser.displayName,
                email: partnerUser.email ?? null,
                avatarUrl: partnerUser.avatarUrl
            };
        }
    }

    return {
        conversationId: conversation._id.toString(),
        type: conversation.type,
        partner,
        orgId: conversation.orgId || null,
        lastMessage: conversation.lastMessageContent ? {
            senderId: conversation.senderId,
            content: conversation.lastMessageContent,
            timestamp: conversation.lastMessageAt
        } : null,
        unreadCount: participant.unreadCount || 0,
        createdAt: conversation.createdAt,
        updatedAt: conversation.updatedAt
    };
};

export const updateConversation = async (id: string, userId: string, data: Partial<IConversation>): Promise<ConversationResponse | null> => {
    // Check if user is a participant
    const participant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    const conversation = await Conversation.findByIdAndUpdate(
        id, 
        data, 
        { new: true }
    ).lean().exec();

    if (!conversation) {
        throw ConversationErrors.NotFound;
    }

    return getConversationById(id, userId);
};

const updateTicketStatus = async (id: string, ticketStatus: string): Promise<IConversation | null> => {
    const objectId = toObjectIdOrNull(id);
    const conversation = await Conversation.findById(objectId).lean().exec();
    
    if (!conversation || conversation.deletedAt) {
        throw ConversationErrors.NotFound;
    }

    if (conversation.type !== ConversationType.ORGANIZATION) {
        throw ConversationErrors.InvalidConversationType;
    }

    const updated = await Conversation.findByIdAndUpdate(
        id, 
        { ticketStatus }, 
        { new: true }
    ).exec();

    return updated;
}

export const assignStaff = async (id: string, staffId: string): Promise<ConversationResponse | null> => {
    const updateTicket = await updateTicketStatus(id, TicketStatus.ASSIGNED);

    if (!updateTicket) {
        throw ConversationErrors.updateTicketStatusFailed;
    }

    await ConversationParticipant.findOneAndUpdate(
        {
            conversationId: id,
            orgId: { $ne: null }
        },
        { memberId: staffId},
    );
    return getConversationById(id, staffId);
}

export const resolveTicket = async (id: string, staffId: string): Promise<ConversationResponse | null> => {
    const updateTicket = await updateTicketStatus(id, TicketStatus.RESOLVED);
    if (!updateTicket) {
        throw ConversationErrors.updateTicketStatusFailed;
    }
    return getConversationById(id, staffId);
};

export const queueTicket = async (id: string): Promise<boolean> => {
    const updateTicket = await updateTicketStatus(id, TicketStatus.QUEUED);
    if (!updateTicket) {
        throw ConversationErrors.updateTicketStatusFailed;
    }
    // Get response BEFORE removing staffId from participant, otherwise getConversationById will throw Unauthorized
    
    await ConversationParticipant.findOneAndUpdate(
        {
            conversationId: id,
            orgId: { $ne: null }
        },
        { memberId: null },
    );
    return true;
}
export const deleteConversation = async (id: string, userId: string): Promise<void> => {
    // Check if user is a participant
    const participant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    const conversation = await Conversation.findByIdAndUpdate(
        id, 
        { deletedAt: new Date() }, 
        { new: true }
    ).exec();

    if (!conversation) {
        throw ConversationErrors.NotFound;
    }
};
export const lookupPartnerStages = (userId: string) => [
    {
        $lookup: {
            from: 'conversationparticipants',
            localField: 'conversationId',
            foreignField: 'conversationId',
            as: 'allParticipants'
        }
    },
    {
        $addFields: {
            partnerParticipant: {
                $first: {
                    $filter: {
                        input: '$allParticipants',
                        cond: {
                            $and: [
                                { $ne: ['$$this.memberId', userId] },
                                { $eq: ['$$this.deletedAt', null] }
                            ]
                        }
                    }
                }
            }
        }
    },
    {
        $lookup: {
            from: 'users',
            localField: 'partnerParticipant.memberId',
            foreignField: '_id',
            as: 'partnerUser'
        }
    },
    {
        $addFields: { partnerUser: { $first: '$partnerUser' } }
    },
];

export const projectConversationStage = {
    $project: {
        conversationId: '$conversation._id',
        type: '$conversation.type',
        orgId: '$conversation.orgId',
        ticketStatus: '$conversation.ticketStatus',
        assignedStaffId: '$conversation.assignedStaffId',
        lastMessageContent: '$conversation.lastMessageContent',
        lastMessageAt: '$conversation.lastMessageAt',
        lastMessageSenderId: '$conversation.senderId',
        unreadCount: { $ifNull: ['$unreadCount', 0] },
        partner: {
            $cond: {
                if: '$partnerUser',
                then: {
                    id: '$partnerUser._id',
                    displayName: { $ifNull: ['$partnerUser.displayName', null] },
                    email: { $ifNull: ['$partnerUser.email', null] },
                    avatarUrl: { $ifNull: ['$partnerUser.avatarUrl', null] },
                },
                else: null
            }
        },
        createdAt: '$conversation.createdAt',
        updatedAt: '$conversation.updatedAt',
    }
};

export const listConversationsByUserId = async (
    userId: string,
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await ConversationParticipant.aggregate([
        {
            $match: { memberId: userId, deletedAt: null }
        },
        {
            $lookup: { from: 'conversations', localField: 'conversationId', foreignField: '_id', as: 'conversation' }
        },
        { $unwind: '$conversation' },
        {
            $match: {
                'conversation.deletedAt': null,
                ...(params.cursor && {
                    'conversation.lastMessageAt': { $lt: new Date(params.cursor) }
                })
            }
        },
        { $sort: { 'conversation.lastMessageAt': -1 } },
        { $limit: limit + 1 },
        ...lookupPartnerStages(userId),
        projectConversationStage,
    ]);

    return formatConversationResult(results, limit);
};

const formatConversationResult = (results: any[], limit: number): ConversationsListResult => {
    const { items, nextCursor, hasMore } = buildPaginatedResult(results, limit, 'lastMessageAt');

    return {
        conversations: items.map(c => ({
            conversationId: c.conversationId.toString(),
            type: c.type,
            orgId: c.orgId ?? null,
            partner: c.partner ?? null,
            ticketStatus: c.ticketStatus ?? null,
            assignedStaffId: c.assignedStaffId ?? null,
            lastMessage: c.lastMessageContent ? {
                senderId: c.lastMessageSenderId,
                content: c.lastMessageContent,
                timestamp: c.lastMessageAt,
            } : null,
            unreadCount: c.unreadCount,
            createdAt: c.createdAt,
            updatedAt: c.updatedAt,
        })),
        nextCursor,
        hasMore,
    };
};

export const listConversationsQueueByStaff = async (
    orgId: string,
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                orgId,
                ticketStatus: TicketStatus.QUEUED,
                deletedAt: null,
                ...(params.cursor && {
                    lastMessageAt: { $lt: new Date(params.cursor) }
                })
            }
        },
        { $sort: { lastMessageAt: -1 } },
        { $limit: limit + 1 },
        { $addFields: { conversationId: '$_id', conversation: '$$ROOT' } },
        ...lookupPartnerStages(orgId),
        projectConversationStage,
    ]);

    return formatConversationResult(results, limit);
};

export const listConversationsAssignedByStaff = async (
    staffId: string,
    orgId: string,
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                orgId,
                // assignedStaffId: staffId,
                ticketStatus: TicketStatus.ASSIGNED,
                deletedAt: null,
                ...(params.cursor && {
                    lastMessageAt: { $lt: new Date(params.cursor) }
                })
            }
        },
        { $sort: { lastMessageAt: -1 } },
        { $limit: limit + 1 },
        { $addFields: { conversationId: '$_id', conversation: '$$ROOT' } },
        ...lookupPartnerStages(staffId),
        projectConversationStage,
    ]);

    return formatConversationResult(results, limit);
};

