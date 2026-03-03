import mongoose from 'mongoose';
import { CreateConversationRequest } from "@/dtos/conversation/conversation.request";
import Conversation, { ConversationType, IConversation } from "@/models/conversation";
import ConversationParticipant, { ConversationParticipantRole } from "@/models/conversation-participant";
import { ConversationErrors } from "./errors/conversation.errors";
import { ConversationResponse } from "@/dtos/conversation/conversation.response";
import User from "@/models/user.model";

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

  // Create conversation without transaction (MongoDB standalone mode)
  const conversation = new Conversation({
    type: data.type,
    ...(data.type === ConversationType.ORGANIZATION && {
      orgId: data.orgId,
      ticketStatus: data.ticketStatus,
    }),
  });
  await conversation.save();

  const participants = buildParticipants(conversation._id, data, userId);
  await ConversationParticipant.insertMany(participants);

  return conversation;
};

const buildParticipants = (conversationId: mongoose.Types.ObjectId, data: CreateConversationRequest, userId: string) => {
  if (data.type === ConversationType.ORGANIZATION) {
    return [
      { conversationId, memberId: userId, role: ConversationParticipantRole.CUSTOMER, orgId: data.orgId },
      { conversationId, memberId: data.orgId, role: ConversationParticipantRole.STAFF, orgId: data.orgId },
    ];
  }
  return [
    { conversationId, memberId: userId, role: ConversationParticipantRole.CUSTOMER },
    { conversationId, memberId: data.memberId, role: ConversationParticipantRole.CUSTOMER },
  ];
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

    let partner = null;

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
                email: partnerUser.email,
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

    // Return formatted response
    return getConversationById(id, userId);
};

export const updateTicketStatus = async (id: string, userId: string, ticketStatus: string): Promise<ConversationResponse | null> => {
    // Check if conversation exists and is organization type
    const conversation = await Conversation.findById(id).lean().exec();
    
    if (!conversation || conversation.deletedAt) {
        throw ConversationErrors.NotFound;
    }

    if (conversation.type !== ConversationType.ORGANIZATION) {
        throw ConversationErrors.InvalidConversationType;
    }

    // Check if user is a participant
    const participant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    await Conversation.findByIdAndUpdate(
        id, 
        { ticketStatus }, 
        { new: true }
    ).exec();

    // Return formatted response
    return getConversationById(id, userId);
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

export interface ConversationsListResult {
    conversations: ConversationResponse[];
    nextCursor: string | null;
    hasMore: boolean;
}

export const listConversationsByUserId = async (
    userId: string,
    params: { cursor?: string; limit?: number } = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(params.limit || 20, 100);

    // Get all conversation participants for this user
    const participantRecords = await ConversationParticipant.find({
        memberId: userId,
        deletedAt: null
    }).exec();

    const conversationIds = participantRecords.map(p => p.conversationId);

    const query: any = {
        _id: { $in: conversationIds },
        deletedAt: null,
    };

    // Cursor by lastMessageAt (ISO date string of last item from previous page)
    if (params.cursor) {
        query.lastMessageAt = { $lt: new Date(params.cursor) };
    }

    // Fetch limit + 1 to detect hasMore
    const conversations = await Conversation.find(query)
        .sort({ lastMessageAt: -1 })
        .limit(limit + 1)
        .lean()
        .exec();

    const hasMore = conversations.length > limit;
    if (hasMore) {
        conversations.pop();
    }

    const lastConversation = conversations[conversations.length - 1];
    const nextCursor = hasMore && lastConversation?.lastMessageAt
        ? lastConversation.lastMessageAt.toISOString()
        : null;

    // Map to response format
    const responses: ConversationResponse[] = [];

    for (const conversation of conversations) {
        const participant = participantRecords.find(
            p => p.conversationId.toString() === conversation._id.toString()
        );

        let partner = null;

        // Get the other participant (partner) for both personal and organization conversations
        const otherParticipant = await ConversationParticipant.findOne({
            conversationId: conversation._id,
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
                    email: partnerUser.email,
                    avatarUrl: partnerUser.avatarUrl
                };
            }
        }

        responses.push({
            conversationId: conversation._id.toString(),
            type: conversation.type,
            partner,
            orgId: conversation.orgId || null,
            lastMessage: conversation.lastMessageContent ? {
                senderId: conversation.senderId,
                content: conversation.lastMessageContent,
                timestamp: conversation.lastMessageAt
            } : null,
            unreadCount: participant?.unreadCount || 0,
            createdAt: conversation.createdAt,
            updatedAt: conversation.updatedAt
        });
    }

    return { conversations: responses, nextCursor, hasMore };
};
