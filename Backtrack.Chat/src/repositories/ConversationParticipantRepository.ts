import ConversationParticipant, {
  IConversationParticipant,
  ParticipantRole,
} from '@src/models/ConversationParticipants';
import { ConversationType } from '@src/models/Conversation';
import { UserInfo } from '@src/models/common/types';
import { BaseRepository } from './base/BaseRepository';
import { IBaseRepository } from './base/IBaseRepository';

export interface IConversationParticipantRepository
  extends IBaseRepository<IConversationParticipant> {
  findByUserId(userId: string): Promise<IConversationParticipant[]>;
  findByConversationId(
    conversationId: string,
  ): Promise<IConversationParticipant[]>;

  isParticipant(conversationId: string, userId: string): Promise<boolean>;

  findExistingSingleConversation(
    userId1: string,
    userId2: string,
  ): Promise<string | null>;

  addParticipants(
    conversationId: string,
    participants: {
      user: UserInfo,
      role: ParticipantRole,
    }[],
  ): Promise<IConversationParticipant[]>;

  removeParticipant(conversationId: string, userId: string): Promise<boolean>;
}

class ConversationParticipantRepository
  extends BaseRepository<IConversationParticipant>
  implements IConversationParticipantRepository
{
  constructor() {
    super(ConversationParticipant);
  }

  /**
   * Find all participants for a user
   */
  public async findByUserId(
    userId: string,
  ): Promise<IConversationParticipant[]> {
    return this.find({ 'user.id': userId });
  }

  /**
   * Find all participants in a conversation
   */
  public async findByConversationId(
    conversationId: string,
  ): Promise<IConversationParticipant[]> {
    return this.find({ conversationId });
  }

  /**
   * Check if user is a participant (called on every message send/receive)
   */
  public async isParticipant(
    conversationId: string,
    userId: string,
  ): Promise<boolean> {
    return await this.exists({
      conversationId,
      'user.id': userId,
    });
  }

  /**
   * Find existing SINGLE conversation between two users
   * CRITICAL: Uses single aggregation query instead of N queries
   * This fixes the major N+1 query problem
   *
   * Previous implementation made 2N queries:
   * - 1 query to get all conversation IDs
   * - N queries to get participants for each conversation
   * - N queries to get conversation details
   *
   * New implementation: 1 aggregation query total
   */
  public async findExistingSingleConversation(
    userId1: string,
    userId2: string,
  ): Promise<string | null> {
    const result = await this.model.aggregate<{ _id: string }>([
      // Step 1: Match participants for either user (only non-deleted)
      {
        $match: {
          'user.id': { $in: [userId1, userId2] },
          deletedAt: null,
        },
      },
      // Step 2: Group by conversation to count participants
      {
        $group: {
          _id: '$conversationId',
          participants: { $push: '$user.id' },
          count: { $sum: 1 },
        },
      },
      // Step 3: Only conversations with exactly 2 participants
      {
        $match: {
          count: 2,
        },
      },
      // Step 4: Check if both users are in the participant list
      {
        $match: {
          participants: { $all: [userId1, userId2] },
        },
      },
      // Step 5: Lookup conversation to check type
      {
        $lookup: {
          from: 'conversations',
          localField: '_id',
          foreignField: '_id',
          as: 'conversation',
        },
      },
      {
        $unwind: '$conversation',
      },
      // Step 6: Only SINGLE type conversations that aren't deleted
      {
        $match: {
          'conversation.type': ConversationType.SINGLE,
          'conversation.deletedAt': null,
        },
      },
      // Step 7: Return just the conversation ID
      {
        $project: {
          _id: 1,
        },
      },
      {
        $limit: 1,
      },
    ]);

    return result.length > 0 ? result[0]._id : null;
  }

  /**
   * Add multiple participants to a conversation
   */
  public async addParticipants(
    conversationId: string,
    participants: { user: UserInfo, role: ParticipantRole }[],
  ): Promise<IConversationParticipant[]> {
    if (participants.length === 0) {
      return [];
    }

    const docs = participants.map((p) => ({
      conversationId,
      user: p.user,
      role: p.role,
    }));

    return await this.createMany(docs);
  }

  /**
   * Remove a participant from a conversation (soft delete)
   */
  public async removeParticipant(
    conversationId: string,
    userId: string,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({
        conversationId,
        'user.id': userId,
      }),
      { deletedAt: new Date() },
    );

    return result.modifiedCount > 0;
  }
}

export default new ConversationParticipantRepository();
