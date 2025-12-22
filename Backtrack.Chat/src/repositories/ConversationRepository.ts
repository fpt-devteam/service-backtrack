import Conversation, {
  ConversationType,
  IConversation,
} from '@src/models/Conversation';
import { BaseRepository } from './base/BaseRepository';
import { IBaseRepository } from './base/IBaseRepository';

export interface IConversationRepository extends IBaseRepository<IConversation> {
  findByIds(ids: string[]): Promise<IConversation[]>;
  findByCreatorId(creatorId: string): Promise<IConversation[]>;
  findByType(type: ConversationType): Promise<IConversation[]>;
}

class ConversationRepository
  extends BaseRepository<IConversation>
  implements IConversationRepository
{
  constructor() {
    super(Conversation);
  }

  /**
   * Batch fetch conversations by IDs
   */
  public async findByIds(ids: string[]): Promise<IConversation[]> {
    if (ids.length === 0) {
      return [];
    }

    const conversations = await this.model
      .find(
        this.addSoftDeleteFilter({
          _id: { $in: ids },
        }),
      )
      .lean();

    return conversations as unknown as IConversation[];
  }

  /**
   * Find conversations by creator ID
   */
  public async findByCreatorId(
    creatorId: string,
  ): Promise<IConversation[]> {
    return this.find({ 'createdBy.id': creatorId });
  }

  /**
   * Find conversations by type
   */
  public async findByType(
    type: ConversationType,
  ): Promise<IConversation[]> {
    return this.find({ type });
  }
}

export default new ConversationRepository();
