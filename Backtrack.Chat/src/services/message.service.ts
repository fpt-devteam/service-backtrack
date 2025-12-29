import {
  messageRepository,
  conversationRepository,
  participantRepository,
  userRepository,
} from '@src/repositories';
import {
  ErrorCodes,
} from '@src/common/errors';
import { MessageType, IMessage } from '@src/models/message.model';
import { PaginationOptions } from '@src/contracts/requests/pagination.request';
import {
  PaginatedResponse } from '@src/contracts/responses/pagination.response';
import { MessageResponse } from '@src/contracts/responses/message.response';

class MessageService {

  private toMessageResponse(message: IMessage): MessageResponse {
    return {
      id: message._id.toString(),
      // eslint-disable-next-line @typescript-eslint/no-base-to-string
      conversationId: message.conversationId.toString(),
      senderId: message.senderId,
      type: message.type,
      content: message.content,
      status: message.status,
      createdAt: message.createdAt.toISOString(),
      updatedAt: message.updatedAt.toISOString(),
    };
  }

  public async sendMessage(
    senderId: string,
    conversationId: string,
    content: string,
    typeContent?: MessageType,
  ): Promise<MessageResponse> {
    if (!content.trim()) {
      throw ErrorCodes.EmptyMessageContent;
    }

    const conversation = await conversationRepository.findById(conversationId);
    if (!conversation) {
      throw ErrorCodes.ConversationNotFound;
    }

    const isParticipant = await participantRepository.isParticipant(
      conversationId,
      senderId,
    );

    if (!isParticipant) {
      throw ErrorCodes.NotParticipant;
    }

    const sender = await userRepository.getByIdAsync?.(senderId);
    if (!sender) {
      throw ErrorCodes.SenderNotFound;
    }

    const message = await messageRepository.create({
      conversationId,
      senderId,
      content,
      type: typeContent ?? MessageType.TEXT,
    });

    await this.updateConversationMetadata(
      conversationId,
      senderId,
      content,
      message.createdAt,
    );

    return this.toMessageResponse(message);
  }

  private async updateConversationMetadata(
    conversationId: string,
    senderId: string,
    content: string,
    timestamp: Date,
  ): Promise<void> {
    await conversationRepository.updateLastMessage(
      conversationId,
      content,
      timestamp,
      senderId,
    );

    const participants = await participantRepository.findByConversationId(
      conversationId,
    );

    const updatePromises = participants.map((p) => {
      if (p.memberId === senderId) {
        return participantRepository.updateTimestamp(
          conversationId, p.memberId);
      } else {
        return participantRepository.incrementUnreadCount(
          conversationId,
          p.memberId,
        );
      }
    });

    await Promise.all(updatePromises);
  }

  public async getMessagesByConversationId(
    conversationId: string,
    userId: string,
    options: PaginationOptions,
  ): Promise<PaginatedResponse<MessageResponse[]>> {
    const { limit = 10, cursor } = options;
    const conversation = await conversationRepository.findById(conversationId);
    if (!conversation || conversation.deletedAt) {
      throw ErrorCodes.ConversationNotFound;
    }

    const isParticipant = await participantRepository.isParticipant(
      conversationId,
      userId,
    );

    if (!isParticipant) {
      throw ErrorCodes.NotParticipant;
    }

    const messages = await messageRepository.findMessagesPaginated(
      conversationId,
      limit,
      cursor,
    );

    const hasMore = messages.length > limit;
    const items = hasMore ? messages.slice(0, limit) : messages;

    if (items.length === 0) {
      return {
        items: [],
        hasMore: false,
        nextCursor: null,
      };
    }

    const messageResponses = items.map(msg => this.toMessageResponse(msg));

    const nextCursor = hasMore && items.length > 0
      ? items[items.length - 1].createdAt.toISOString()
      : null;

    return {
      items: messageResponses,
      hasMore,
      nextCursor,
    };
  }

  public async getMessageCount(conversationId: string): Promise<number> {
    return await messageRepository.countByConversationId(conversationId);
  }
}

export default new MessageService();
