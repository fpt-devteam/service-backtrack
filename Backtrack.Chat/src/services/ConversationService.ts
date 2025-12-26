import {
  conversationRepository,
  participantRepository,
} from '@src/repositories';
import userRepository from '@src/repositories/UserRepository';
import { NotFoundError, BadRequestError } from '@src/common/errors';
import { ConversationType } from '@src/models/Conversation';
import { ParticipantRole } from '@src/models/ConversationParticipants';

export class ConversationService {
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
            ? otherParticipant.user.displayName
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
    creatorId: string,
    type: ConversationType,
    participantIds: string[],
    name?: string,
  ) {
    const creator = await userRepository.getByIdAsync(creatorId);
    if (!creator) {
      throw new NotFoundError('Creator user not found');
    }

    const participantUsers = await userRepository.findByIds(participantIds);

    if (participantUsers.length !== participantIds.length) {
      const foundIds = participantUsers.map((u) => u._id);
      const missingIds = participantIds.filter((id) => !foundIds.includes(id));
      throw new NotFoundError(`Users not found: ${missingIds.join(', ')}`);
    }

    if (type === ConversationType.SINGLE) {
      const existingId =
        await participantRepository.findExistingSingleConversation(
          creatorId,
          participantIds[0],
        );

      if (existingId) {
        throw new BadRequestError('Single conversation already exists');
      }
    }

    const conversation = await conversationRepository.create({
      type,
      name: name ?? participantUsers[0]?.displayName ?? null,
      createdBy: {
        id: creator._id,
        displayName: creator.displayName ?? 'Unknown',
        avatarUrl: creator.avatarUrl ?? null,
      },
    });

    // Build participant documents
    const participantDocs = [
      {
        user: {
          id: creator._id,
          displayName: creator.displayName ?? 'Unknown',
          avatarUrl: creator.avatarUrl ?? null,
        },
        role: ParticipantRole.ADMIN,
      },
      ...participantUsers
        .filter((p) => p._id !== creatorId)
        .map((p) => ({
          user: {
            id: p._id,
            displayName: p.displayName ?? 'Unknown',
            avatarUrl: p.avatarUrl ?? null,
          },
          role: ParticipantRole.MEMBER,
        })),
    ];

    await participantRepository.addParticipants(
      conversation._id.toString(),
      participantDocs,
    );

    return conversation;
  }
}

export default new ConversationService();

