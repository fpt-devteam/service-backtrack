import Message, { IMessage } from '@src/models/message.model';
import { BaseRepository } from './base/base.repository';
import {
  IBaseRepository,
} from './base/ibase.repository';
import { Types } from 'mongoose';

export interface IMessageRepository extends IBaseRepository<IMessage> {
  findMessagesPaginated(
    conversationId: string,
    limit: number,
    cursor?: string,
  ): Promise<IMessage[]>;
  countByConversationId(conversationId: string): Promise<number>;
  softDeleteByConversationId(conversationId: string): Promise<number>;
}

export class MessageRepository
  extends BaseRepository<IMessage>
  implements IMessageRepository {
  public constructor() {
    super(Message);
  }
  public async findMessagesPaginated(
    conversationId: string,
    limit: number,
    cursor?: string,
  ): Promise<IMessage[]> {
    const query: Record<string, unknown> = {
      conversationId: new Types.ObjectId(conversationId),
      deletedAt: null,
      ...(cursor && { createdAt: { $lt: new Date(cursor) } }),
    };
    const messages = await this.model
      .find(query)
      .sort({ createdAt: -1 })
      .limit(limit + 1)
      .lean()
      .exec();

    return messages as unknown as IMessage[];
  }
  /**
   * Count messages in a conversation
   */
  public async countByConversationId(conversationId: string): Promise<number> {
    return await this.count({ conversationId });
  }

  /**
   * Soft delete all messages in a conversation
   */
  public async softDeleteByConversationId(
    conversationId: string,
  ): Promise<number> {
    const result = await this.model.updateMany(
      this.addSoftDeleteFilter({ conversationId }),
      { deletedAt: new Date() },
    );

    return result.modifiedCount;
  }
}

