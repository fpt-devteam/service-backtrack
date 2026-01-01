import ConversationParticipant, {
  IConversationParticipant,
} from '@src/models/conversation-participant.model';
import { BaseRepository } from './base/base.repository';
import { IBaseRepository } from './base/ibase.repository';
import { FilterQuery } from 'mongoose';
import logger from '@src/utils/logger';

export interface IConversationParticipantRepository
  extends IBaseRepository<IConversationParticipant> {
  findByUserId(userId: string): Promise<IConversationParticipant[]>;
  findByUserIdPaginated(
    userId: string,
    limit: number,
    cursor?: string,
  ): Promise<IConversationParticipant[]>;
  findPartnerParticipantsByConversationIds(
    conversationIds: string[],
    excludeUserId: string,
  ): Promise<IConversationParticipant[]>;
  findByConversationId(
    conversationId: string,
  ): Promise<IConversationParticipant[]>;
  findPartner(
    conversationId: string,
    currentUserId: string,
  ): Promise<IConversationParticipant | null>;
  findExistingConversation(
    userId1: string,
    userId2: string,
  ): Promise<string | null>;
  isParticipant(conversationId: string, userId: string): Promise<boolean>;
  addParticipants(
    conversationId: string,
    participantsReq: Record<string, string>,
  ): Promise<IConversationParticipant[] | undefined>;
  incrementUnreadCount(
    conversationId: string,
    userId: string,
    amount?: number,
  ): Promise<boolean>;
  resetUnreadCount(
    conversationId: string,
    userId: string,
    lastReadMessageId?: string,
  ): Promise<boolean>;
  updateTimestamp(
    conversationId: string,
    userId: string,
  ): Promise<boolean>;
  setNickname(
    conversationId: string,
    userId: string,
    nickname: string | null,
  ): Promise<boolean>;
  removeParticipant(
    conversationId: string,
    userId: string,
  ): Promise<boolean>;
}

export class ConversationParticipantRepository
  extends BaseRepository<IConversationParticipant>
  implements IConversationParticipantRepository
{
  public constructor() {
    super(ConversationParticipant);
  }

  public async findExistingConversation(
    userId1: string,
    userId2: string,
  ): Promise<string | null> {
    const result = await this.model.aggregate([
      {
        $match: {
          memberId: { $in: [userId1, userId2] },
          deletedAt: null,
        },
      },
      {
        $group: {
          _id: '$conversationId',
          members: { $addToSet: '$memberId' },
          count: { $sum: 1 },
        },
      },
      {
        $match: {
          count: 2, 
          members: { $all: [userId1, userId2] }, 
        },
      },
      {
        $limit: 1,
      },
    ]);

    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    return result.length > 0 ? result[0]._id as string : null;
  }

  public findPartner(
    conversationId: string,
    currentUserId: string,
  ): Promise<IConversationParticipant | null> {
    return this.findOne({
      conversationId: conversationId,
      memberId: { $ne: currentUserId },
    });
  }

  public async findByUserId(
    userId: string,
  ): Promise<IConversationParticipant[]> {
    return this.find({ memberId: userId });
  }

  public async findByUserIdPaginated(
    userId: string,
    limit: number,
    cursor?: string,
  ): Promise<IConversationParticipant[]> {
    try {
      const baseFilter: FilterQuery<IConversationParticipant> = {
        memberId: userId,
      };

      if (cursor) {
        baseFilter.updatedAt = { $lt: new Date(cursor) };
      }

      const result = await this.model
        .find(this.addSoftDeleteFilter(baseFilter))
        .sort({ updatedAt: -1 })
        .limit(limit)
        .lean();

      return result as unknown as IConversationParticipant[];
    } catch (error) {
      logger.error('Error finding paginated participants:', error);
      throw error;
    }
  }

  public async findPartnerParticipantsByConversationIds(
    conversationIds: string[],
    excludeUserId: string,
  ): Promise<IConversationParticipant[]> {
    if (conversationIds.length === 0) {
      return [];
    }

    const result = await this.model
      .find({
        conversationId: {
          $in: conversationIds.map((id) => id),
        },
        memberId: { $ne: excludeUserId },
        deletedAt: null,
      })
      .lean();
    return result as unknown as IConversationParticipant[];
  }

  public async findByConversationId(
    conversationId: string,
  ): Promise<IConversationParticipant[]> {
    return this.find({ conversationId: conversationId });
  }

  public async incrementUnreadCount(
    conversationId: string,
    userId: string,
    amount?: number,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({
        conversationId: conversationId,
        memberId: userId,
      }),
      {
        $inc: { unreadCount: amount ?? 1 },
        $set: { updatedAt: new Date() },
      },
    );

    return result.modifiedCount > 0;
  }

  public async resetUnreadCount(
    conversationId: string,
    userId: string,
    lastReadMessageId?: string,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({
        conversationId: conversationId,
        memberId: userId,
      }),
      {
        $set: {
          unreadCount: 0,
          lastReadAt: new Date(),
          lastReadMessageId: lastReadMessageId ?? null,
        },
      },
    );
    return result.modifiedCount > 0;
  }

  public async setNickname(
    conversationId: string,
    userId: string,
    nickname: string | null,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({
        conversationId: conversationId,
        memberId: userId,
      }),
      {
        $set: {
          nickName: nickname,
        },
      },
    );
    return result.modifiedCount > 0;
  }

  public async isParticipant(
    conversationId: string,
    userId: string,
  ): Promise<boolean> {
    const result = await this.model.findOne(
      this.addSoftDeleteFilter({
        conversationId,
        memberId: userId,
      }),
    ).lean();

    return result !== null;
  }

  public async addParticipants(
    conversationId: string,
    participantsReq: Record<string, string>,
  ): Promise<IConversationParticipant[] | undefined> {
    if (!participantsReq || Object.keys(participantsReq).length === 0) {
      return undefined;
    }

    const docs = Object.keys(participantsReq).map(participantId => ({
      conversationId: conversationId,
      memberId: participantId,
      nickName: participantsReq[participantId],
    })) as IConversationParticipant[];

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
        memberId: userId,
      }),
      { $set: { deletedAt: new Date() } },
    );

    return result.modifiedCount > 0;
  }

  public async updateTimestamp(
    conversationId: string,
    userId: string,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({
        conversationId,
        memberId: userId,
      }),
      { $set: { updatedAt: new Date() } },
    );

    return result.modifiedCount > 0;
  }
}

