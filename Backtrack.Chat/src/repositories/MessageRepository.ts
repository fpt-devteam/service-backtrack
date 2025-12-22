import Message, { IMessage } from '@src/models/Message';
import { BaseRepository } from './base/BaseRepository';
import {
  IBaseRepository,
  PaginationOptions,
  PaginatedResult,
} from './base/IBaseRepository';

export interface IMessageRepository extends IBaseRepository<IMessage> {
  findByConversationId(
    conversationId: string,
    options: PaginationOptions
  ): Promise<PaginatedResult<IMessage>>;

  countByConversationId(conversationId: string): Promise<number>;

  findBySenderId(
    senderId: string,
    options: PaginationOptions
  ): Promise<PaginatedResult<IMessage>>;

  softDeleteByConversationId(conversationId: string): Promise<number>;
}

class MessageRepository
  extends BaseRepository<IMessage>
  implements IMessageRepository
{
  public constructor() {
    super(Message);
  }

  /**
   * Find messages by conversation ID with cursor-based pagination
   */
  public async findByConversationId(
    conversationId: string,
    options: PaginationOptions,
  ): Promise<PaginatedResult<IMessage>> {
    return this.findWithPagination(
      { conversationId },
      {
        ...options,
        sortField: 'timestamp',
        sortOrder: options.sortOrder ?? 'asc',
      },
    );
  }

  /**
   * Count messages in a conversation
   */
  public async countByConversationId(conversationId: string): Promise<number> {
    return await this.count({ conversationId });
  }

  /**
   * Find messages by sender with pagination
   */
  public async findBySenderId(
    senderId: string,
    options: PaginationOptions,
  ): Promise<PaginatedResult<IMessage>> {
    return this.findWithPagination(
      { 'sender.id': senderId },
      {
        ...options,
        sortField: 'timestamp',
        sortOrder: options.sortOrder ?? 'desc',
      },
    );
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

export default new MessageRepository();
