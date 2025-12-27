import {
  messageRepository,
  conversationRepository,
  participantRepository,
  userRepository,
} from '@src/repositories';
import {
  NotFoundError,
  ForbiddenError,
  BadRequestError,
} from '@src/common/errors';
import { MessageType } from '@src/models/message.model';

class MessageService {
  public async sendMessage(
    senderId: string,
    // sendername: string,
    conversationId: string,
    content: string,
  ) {
    if (!content.trim()) {
      throw new BadRequestError('Message content cannot be empty');
    }

    const conversation = await conversationRepository.findById(conversationId);
    if (!conversation) {
      throw new NotFoundError('Conversation not found');
    }

    const isParticipant = await participantRepository.isParticipant(
      conversationId,
      senderId,
    );

    if (!isParticipant) {
      throw new ForbiddenError('You are not a member of this conversation');
    }

    // Verify sender exists
    const sender = await userRepository.getByIdAsync?.(senderId);
    if (!sender) {
      throw new NotFoundError('Sender user not found');
    }

    // Create message with simplified schema (senderId only)
    const message = await messageRepository.create({
      conversationId,
      senderId,
      content,
      type: MessageType.TEXT,
    });

    return message;
  }

  public async getMessagesByConversationId(
    conversationId: string,
    userId: string,
    cursor?: string,
    limit?: number,
  ) {
    // Check if conversation exists (cached)
    const conversation = await conversationRepository.findById(conversationId);
    if (!conversation) {
      throw new NotFoundError('Conversation not found');
    }

    // Check if user is a participant (cached)
    const isParticipant = await participantRepository.isParticipant(
      conversationId,
      userId,
    );

    if (!isParticipant) {
      throw new ForbiddenError('You are not a member of this conversation');
    }

    // Get paginated messages
    const result = await messageRepository.findByConversationId(
      conversationId,
      { cursor, limit, sortField: 'timestamp', sortOrder: 'asc' },
    );

    return result;
  }

  public async getMessageCount(conversationId: string): Promise<number> {
    return await messageRepository.countByConversationId(conversationId);
  }
}

export default new MessageService();
