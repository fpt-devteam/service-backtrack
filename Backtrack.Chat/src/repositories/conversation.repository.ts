import Conversation, {
  IConversation,
} from '@src/models/conversation.model';
import { BaseRepository } from './base/base.repository';
import { IBaseRepository } from './base/ibase.repository';
import { Types } from 'mongoose';

export interface IConversationRepository 
  extends IBaseRepository<IConversation> {
  findByIds(ids: string[]): Promise<IConversation[]>;
  create(data: Partial<IConversation>): Promise<IConversation>;
  updateLastMessage(
    conversationId: string,
    messageContent: string,
    messageTimestamp: Date
  ): Promise<boolean>;
  softDelete(conversationId: string): Promise<boolean>;
  existsConversation(conversationId: string): Promise<boolean>;
}

export class ConversationRepository
  extends BaseRepository<IConversation>
  implements IConversationRepository
{
  public constructor() {
    super(Conversation);
  }
  public async existsConversation(conversationId: string): Promise<boolean> {
    const count = await this.count({ _id: conversationId });
    return count > 0;
  }

  public async updateLastMessage(
    conversationId: string,
    messageContent: string,
    messageTimestamp: Date,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({ _id: conversationId }),
      {
        $set: {
          lastMessageContent: messageContent.substring(0, 200), 
          lastMessageAt: messageTimestamp,
          updatedAt: new Date(),
        },
      },
    );

    return result.modifiedCount > 0;
  }

  public async findByIds(
    ids: (string | Types.ObjectId)[]): Promise<IConversation[]> {
    if (ids.length === 0) {
      return [];
    }

    const conversations = await this.find({ _id: { $in: ids } });

    return conversations as unknown as IConversation[];
  }
}