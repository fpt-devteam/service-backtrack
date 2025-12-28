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
import { Types } from 'mongoose';

export class ConversationService {
  public async getAllConversationsByUserId(
    userId: string,
    options: PaginationOptions = {},
  ): Promise<PaginatedResponse<ConversationResponse[]>> {
    const { limit = 20, cursor } = options;

    const participants = await participantRepository.findByUserIdPaginated(
      userId,
      limit + 1,
      cursor,
    );

    const hasMore = participants.length > limit;
    const items = hasMore ? participants.slice(0, limit) : participants;

    if (items.length === 0) {
      return {
        items: [],
        hasMore: false,
        nextCursor: null,
      };
    }
    const conversationIds = items.map(p =>  (
      p.conversationId as unknown as Types.ObjectId).toHexString());
    
    const conversations = await conversationRepository.findByIds(
      conversationIds,
    );

    const conversationsMap = new Map(
      conversations.map(c => [c._id.toString(), c]),
    );

    const partnerParticipants = await participantRepository
      .findPartnerParticipantsByConversationIds(
        conversationIds,
        userId,
      );

    const partnerParticipantsMap = new Map(
      partnerParticipants.map(p => [
        typeof p.conversationId === 'string'
          ? p.conversationId
          : (p.conversationId as unknown as Types.ObjectId).toHexString(),
        p,
      ]),
    );

    const partnerIds = [...new Set(
      partnerParticipants.map(p => p.memberId),
    )];

    const partnerUsers = await userRepository.findByIds(partnerIds);
    
    const partnerUsersMap = new Map(
      partnerUsers.map(u => [u._id, u]),
    );

    const conversationResponses: ConversationResponse[] = items.map(
      myParticipant => {
        const conversationId = (
          myParticipant.conversationId as unknown as Types.ObjectId
        ).toHexString();
        const conversation = conversationsMap.get(conversationId);
        const partnerParticipant = partnerParticipantsMap.get(conversationId);
        const partnerUser = partnerParticipant 
          ? partnerUsersMap.get(partnerParticipant.memberId)
          : null;

        const partnerDisplayName =
        partnerParticipant?.partnerDisplayName ??
        partnerUser?.displayName ??
        'Unknown';

        return {
          conversationId,
          partner: {
            id: partnerParticipant?.memberId ?? '',
            displayName: partnerDisplayName,
            avatar: partnerUser?.avatarUrl ?? '',
          },
          lastMessage: {
            lastContent: conversation?.lastMessageContent ?? '',
            timestamp: conversation?.lastMessageAt?.toISOString() ?? '',
          },
          unreadCount: myParticipant.unreadCount ?? 0,
          updatedAt: myParticipant.updatedAt,
        };
      });
    const nextCursor = hasMore 
      ? items[items.length - 1].updatedAt.toISOString()
      : null;

    return {
      items: conversationResponses,
      hasMore,
      nextCursor,
    };
  }

  public async getConversationById(
    conversationId: string,
  ) {
    const [conversation, participants] = await Promise.all([
      conversationRepository.findById(conversationId),
      participantRepository.findByConversationId(conversationId),
    ]);

    if (!conversation) {
      throw ErrorCodes.ConversationNotFound;
    }

    return {
      ...conversation,
      participants,
    };
  }

  public async createConversation(
    request: CreateConversationInput,
  ) {
    const creator = await userRepository.getByIdAsync?.(request.creatorId);
    const partner = await userRepository.getByIdAsync?.(request.partnerId);
    if (!creator) {
      throw ErrorCodes.UserNotFound;
    }
    if (!partner) {
      throw ErrorCodes.PartnerNotFound;
    }

    const existingConversationId = await participantRepository.
      findExistingConversation(
        request.creatorId,
        request.partnerId,
      );

    if (existingConversationId) {
      throw ErrorCodes.ConversationAlreadyExists;
    }

    const conversation = await conversationRepository.create({});

    const buildNickname = (
      keyName?: string,
      displayName?: string | null,
    ): string => {
      const name = displayName ?? 'Unknown';
      return keyName ? `${keyName} - ${name}` : name;
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
