import {
  conversationRepository,
  participantRepository,
  userRepository,
} from '@src/repositories';
import { ErrorCodes } from '@src/common/errors';
import {
  CreateConversationInput,
} from '@src/contracts/requests/conversation.request';
import { PaginationOptions } from '@src/repositories/base/ibase.repository';
import {
  ConversationResponse,
} from '@src/contracts/responses/conversation.response';
import {
  PaginatedResponse } from '@src/contracts/responses/pagination.response';
import { 
  ConversationAggregationResult,
} from '@src/repositories/conversation.repository';

export class ConversationService {
  public async getAllConversationsByUserId(
    userId: string,
    options: PaginationOptions = {},
  ): Promise<PaginatedResponse<ConversationResponse[]>> {
    const { limit = 10, cursor } = options;

    const conversations: ConversationAggregationResult[] = 
    await conversationRepository.findConversationsPaginated(
      userId,
      limit,
      cursor,
    );

    const hasMore = conversations.length > limit;
    const nextCursor = hasMore && conversations.length > 0
      ? conversations[conversations.length - 1].lastMessageAt?.toISOString() 
      ?? null
      : null;
    if (hasMore) conversations.pop();

    const conversationResponses: ConversationResponse[] = 
    conversations.map(conv => ({
      conversationId: conv.conversationId.toString(),
      partner: {
        id: conv.partner.id,
        displayName: conv.partner.displayName,
        avatar: conv.partner.avatar,
      },
      lastMessage: {
        lastContent: conv.lastMessageContent ?? '',
        timestamp: conv.lastMessageAt?.toISOString() ?? '',
        senderId: conv.senderId ?? '',
      },
      unreadCount: conv.myParticipant?.unreadCount ?? 0,
      updatedAt: conv.myParticipant?.updatedAt,
    }));
    return {
      items: conversationResponses,
      hasMore,
      nextCursor,
    };
  }

  public async getConversationById(
    conversationId: string,
    userId: string,
  ): Promise<ConversationResponse> {
    const result = await conversationRepository.findConversationByIdWithDetails(
      conversationId,
      userId,
    );

    if (!result) {
      throw ErrorCodes.ConversationNotFound;
    }

    return {
      conversationId: result.conversationId.toString(),
      partner: {
        id: result.partner.id,
        displayName: result.partner.displayName,
        avatar: result.partner.avatar,
      },
      lastMessage: {
        lastContent: result.lastMessageContent ?? '',
        timestamp: result.lastMessageAt?.toISOString() ?? '',
        senderId: result.senderId ?? '',

      },
      unreadCount: result.myParticipant?.unreadCount ?? 0,
      updatedAt: result.myParticipant?.updatedAt,
    };
  }

  public async createConversation(
    request: CreateConversationInput,
  ) {
    const creator = await userRepository.getByIdAsync(request.creatorId);
    const partner = await userRepository.getByIdAsync(request.partnerId);
    if (!creator) {
      throw ErrorCodes.UserNotFound;
    }
    if (!partner) {
      throw ErrorCodes.PartnerNotFound;
    }

    if (creator._id === partner._id) {
      throw ErrorCodes.CannotCreateConversationWithYourself;
    }

    const existingConversationId = await participantRepository.
      findExistingConversation(
        request.creatorId,
        request.partnerId,
      );

    if (existingConversationId) {
      return existingConversationId;
    }

    const conversation = await conversationRepository.create({});

    const buildNickname = (
      keyName: string,
      displayName?: string | null,
    ): string => {
      if (!displayName) {
        return keyName;
      }
      return `${keyName} - ${displayName}`;
    };

    const participantsReq: Record<string, string> = {
      [request.creatorId]: buildNickname(
        request.creatorKeyName,
        creator.displayName,
      ),
      [request.partnerId]: buildNickname(
        request.partnerKeyName,
        partner.displayName,
      ),
    };

    await participantRepository.addParticipants(
      conversation._id.toString(),
      participantsReq,
    );

    return conversation._id.toString();
  }

}

export default new ConversationService();
