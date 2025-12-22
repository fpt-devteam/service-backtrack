import {
  conversationRepository,
  participantRepository,
} from '@src/repositories';
import { NotFoundError, BadRequestError } from '@src/common/errors';
import { ConversationType } from '@src/models/Conversation';
import { ParticipantRole } from '@src/models/ConversationParticipants';

class ConversationService {
  public async getAllConversationsByUserId(userId: string) {

    const participants = await participantRepository.findByUserId(userId);
    const conversationIds = participants.map((p) => p.conversationId);
    const conversations = await conversationRepository.findByIds(
      conversationIds,
    );

    const conversationsWithParticipants = await Promise.all(
      conversations.map(async (conv) => {
        const convParticipants =
          await participantRepository.findByConversationId(
            conv._id.toString(),
          );

        let displayName = conv.name;
        if (conv.type === 'SINGLE') {
          const otherParticipant = convParticipants.find(
            (p) => p.user.id !== userId,
          );
          displayName = otherParticipant
            ? otherParticipant.user.username
            : 'Unknown';
        }

        return {
          ...conv,
          name: displayName,
          participants: convParticipants,
        };
      }),
    );

    return conversationsWithParticipants.sort(
      (a, b) => b.updatedAt.getTime() - a.updatedAt.getTime(),
    );
  }

  public async getConversationById(conversationId: string) {
    const [conversation, participants] = await Promise.all([
      conversationRepository.findById(conversationId), // Cached
      participantRepository.findByConversationId(conversationId),
    ]);

    if (!conversation) {
      throw new NotFoundError('Conversation not found');
    }

    return {
      ...conversation,
      participants,
    };
  }

  public async createConversation(
    creator: { id: string, username: string, avatarUrl?: string | null },
    type: ConversationType,
    participants: { id: string, username: string, avatarUrl?: string | null }[],
    name?: string,
  ) {
    if (type === ConversationType.SINGLE) {
      const existingId =
        await participantRepository.findExistingSingleConversation(
          creator.id,
          participants[0].id,
        );

      if (existingId) {
        throw new BadRequestError('Single conversation already exists');
      }
    }

    const conversation = await conversationRepository.create({
      type,
      name: name ?? null,
      createdBy: creator,
    });

    const participantDocs = [
      { user: creator, role: ParticipantRole.ADMIN },
      ...participants
        .filter((p) => p.id !== creator.id)
        .map((p) => ({ user: p, role: ParticipantRole.MEMBER })),
    ];

    await participantRepository.addParticipants(
      conversation._id.toString(),
      participantDocs,
    );

    return conversation;
  }
}

export default new ConversationService();
