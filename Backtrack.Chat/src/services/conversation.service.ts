import { CreateConversationRequest } from "@/dtos/conversation/conversation.request";
import Conversation from "@/models/conversation";
import ConversationParticipant from "@/models/conversation-participant";
import ConversationQueue from "@/models/conversation-queue";
import ConversationAssignment from "@/models/conversation-assignment";
import { ConversationErrors } from "./errors/conversation.errors";
import { ConversationPartner, ConversationResponse, ConversationsListResult } from "@/dtos/conversation/conversation.response";
import { ConversationType, IConversation } from "@/models/interfaces/conversation.interface";
import User from "@/models/user.model";
import { Constants } from '@/config/constants';
import { buildPaginatedResult, CursorPaginationParams } from '@/utils/pagination';
import { toStringOrNull } from '@/utils/object-id';
import { createOrgConvParticipants, createPersonalConvParticipants, unassignConversationParticipant } from './conversation-paticipant.service';
import { createConversationQueue, popConversationQueue, pushConversationQueue } from "./conversation-queue.service";
import { ConversationParticipantRole } from "@/models";
import { assignConversation, unassignConversation } from "./conversation-assignment.service";

export const createPersonalConversation = async (
  data: CreateConversationRequest & { type: ConversationType.PERSONAL },
  userId: string
): Promise<IConversation> => {
  const duplicate = await ConversationParticipant.aggregate([
    { $match: { memberId: { $in: [data.memberId, userId] }, deletedAt: null } },
    { $group: { _id: '$conversationId', count: { $sum: 1 } } },
    { $match: { count: 2 } },
    { $limit: 1 },
  ]);

  if (duplicate.length > 0) {
    throw ConversationErrors.AlreadyExists;
  }

  const conversation = new Conversation({ type: data.type });
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) {
    throw ConversationErrors.NotFound;
  }
  await createPersonalConvParticipants(conversationId, data.memberId, userId);
  return conversation;
};

export const createOrgConversation = async (
  data: CreateConversationRequest & { type: ConversationType.ORGANIZATION },
  userId: string
): Promise<IConversation> => {
  // Prevent duplicate: check if an ORGANIZATION conversation with this orgId
  // already has this user as a CUSTOMER participant
  const existingConv = await findExistingOrgConversation(userId, data.orgId);
  if (existingConv) {
    throw ConversationErrors.AlreadyExists;
  }

  const conversation = new Conversation({
    type: data.type,
    orgId: data.orgId,
  });
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) {
    throw ConversationErrors.NotFound;
  }
  await createConversationQueue(conversationId);
  await createOrgConvParticipants(conversationId, ConversationParticipantRole.CUSTOMER, userId);
  return conversation;
};

/**
 * Modern flow: find existing personal conversation between two users,
 * or create a new one if it doesn't exist yet.
 */
export const findOrCreatePersonalConversation = async (
  userId: string,
  recipientId: string,
): Promise<IConversation> => {
  const duplicate = await ConversationParticipant.aggregate([
    { $match: { memberId: { $in: [userId, recipientId] }, deletedAt: null } },
    { $group: { _id: '$conversationId', count: { $sum: 1 } } },
    { $match: { count: 2 } },
    { $limit: 1 },
  ]);

  if (duplicate.length > 0) {
    const existing = await Conversation.findById(duplicate[0]._id).lean().exec();
    if (existing && !existing.deletedAt) return existing;
  }

  const conversation = new Conversation({ type: ConversationType.PERSONAL });
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) throw ConversationErrors.NotFound;
  await createPersonalConvParticipants(conversationId, recipientId, userId);
  return conversation;
};

/**
 * Find an existing ORGANIZATION conversation where userId is a CUSTOMER participant.
 * Correct approach: query by orgId first, then verify user membership.
 * (The inverse — find by userId first — fails when user has chats with multiple orgs.)
 */
const findExistingOrgConversation = async (
  userId: string,
  orgId: string,
): Promise<IConversation | null> => {
  // Get all active conversations for this org
  const orgConversations = await Conversation.find({
    orgId,
    type: ConversationType.ORGANIZATION,
    deletedAt: null,
  }).lean().exec();

  if (!orgConversations.length) return null;

  const convIds = orgConversations.map(c => (c._id as any).toString());

  // Check if this user is a CUSTOMER participant in any of them
  const participant = await ConversationParticipant.findOne({
    conversationId: { $in: convIds },
    memberId: userId,
    role: ConversationParticipantRole.CUSTOMER,
    deletedAt: null,
  }).lean().exec();

  if (!participant) return null;

  return orgConversations.find(
    c => (c._id as any).toString() === participant.conversationId
  ) ?? null;
};

/**
 * Modern flow: find existing org conversation for this user,
 * or create a new one if it doesn't exist yet.
 */
export const findOrCreateOrgConversation = async (
  userId: string,
  orgId: string,
): Promise<IConversation> => {
  const existingConv = await findExistingOrgConversation(userId, orgId);
  if (existingConv) return existingConv;

  const conversation = new Conversation({ type: ConversationType.ORGANIZATION, orgId });
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) throw ConversationErrors.NotFound;
  await createConversationQueue(conversationId);
  await createOrgConvParticipants(conversationId, ConversationParticipantRole.CUSTOMER, userId);
  return conversation;
};


export const getConversationById = async (id: string, userId: string): Promise<ConversationResponse | null> => {
    const conversation = await Conversation.findById(id).lean().exec();

    if (!conversation || conversation.deletedAt) {
        return null;
    }

    const participant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    let partner: ConversationPartner | null = null;

    const otherParticipant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: { $ne: userId },
        deletedAt: null
    }).exec();

    if (otherParticipant?.memberId) {
        const partnerUser = await User.findById(otherParticipant.memberId)
            .select('displayName email avatarUrl')
            .lean()
            .exec();

        if (partnerUser) {
            partner = {
                id: partnerUser._id.toString(),
                displayName: partnerUser.displayName ?? null,
                email: partnerUser.email ?? null,
                avatarUrl: partnerUser.avatarUrl ?? null,
            };
        }
    }

    return {
        conversationId: conversation._id.toString(),
        type: conversation.type,
        partner,
        orgId: conversation.orgId ?? null,
        lastMessage: conversation.lastMessageContent ? {
            senderId: conversation.senderId ?? null,
            content: conversation.lastMessageContent,
            timestamp: conversation.lastMessageAt ?? null,
        } : null,
        unreadCount: participant.unreadCount ?? 0,
        createdAt: conversation.createdAt,
        updatedAt: conversation.updatedAt,
    };
};

export const updateConversation = async (id: string, userId: string, data: Partial<IConversation>): Promise<ConversationResponse | null> => {
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

export const assignStaff = async (id: string, staffId: string): Promise<ConversationResponse | null> => {
    const conversation = await Conversation.findById(id).lean().exec();
    if (!conversation || conversation.deletedAt) throw ConversationErrors.NotFound;
    if (conversation.type !== ConversationType.ORGANIZATION) throw ConversationErrors.InvalidConversationType;

    // Queue entry must exist and not yet been taken by another staff
    const queueEntry = await ConversationQueue.findOne({
        conversationId: id,
        takenAt: null,
        deletedAt: null,
    }).lean().exec();
    if (!queueEntry) throw ConversationErrors.NotInQueue;

    await popConversationQueue(id, staffId);
    await assignConversation(id, staffId);
    await createOrgConvParticipants(id, ConversationParticipantRole.STAFF, staffId);
    await Conversation.findByIdAndUpdate(id, { staffAssignId: staffId });
    return getConversationById(id, staffId);
};

export const queueTicket = async (id: string, staffId: string): Promise<boolean> => {
    const conversation = await Conversation.findById(id).lean().exec();
    if (!conversation || conversation.deletedAt) throw ConversationErrors.NotFound;
    if (conversation.type !== ConversationType.ORGANIZATION) throw ConversationErrors.InvalidConversationType;

    // Staff must be currently assigned to this conversation
    const assignment = await ConversationAssignment.findOne({
        conversationId: id,
        agentId: staffId,
        unassignedAt: null,
    }).lean().exec();
    if (!assignment) throw ConversationErrors.NotAssigned;

    await pushConversationQueue(id);
    await unassignConversation(id, staffId);
    await unassignConversationParticipant(id, ConversationParticipantRole.STAFF);
    await Conversation.findByIdAndUpdate(id, { staffAssignId: null });
    return true;
};
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
            $lookup: {
                from: 'conversations',
                let: { convIdStr: '$conversationId' },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $eq: ['$_id', { $toObjectId: '$$convIdStr' }]
                            }
                        }
                    }
                ],
                as: 'conversation'
            }
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
        {
            $sort: { 'conversation.lastMessageAt': -1 }
        },
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
            lastMessage: c.lastMessageContent ? {
                senderId: c.lastMessageSenderId ?? null,
                content: c.lastMessageContent,
                timestamp: c.lastMessageAt,
            } : null,
            unreadCount: c.unreadCount ?? 0,
            createdAt: c.createdAt,
            updatedAt: c.updatedAt,
        })),
        nextCursor,
        hasMore,
    };
};

/**
 * List conversations in queue for an org.
 * Sorted by lastMessageAt descending.
 */
export const listConversationsQueueByStaff = async (
    orgId: string,
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                orgId,
                staffAssignId: null,      // not yet assigned to any staff
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
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                staffAssignId: staffId,   // currently assigned to this staff
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
