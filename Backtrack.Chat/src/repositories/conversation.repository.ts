import Conversation, {
  IConversation,
} from '@src/models/conversation.model';
import { BaseRepository } from './base/base.repository';
import { IBaseRepository } from './base/ibase.repository';
import { PipelineStage, Types } from 'mongoose';

export interface ConversationAggregationResult {
  _id: Types.ObjectId;
  conversationId: Types.ObjectId;
  lastMessageContent: string;
  lastMessageAt: Date;
  senderId: string;
  myParticipant: {
    unreadCount: number,
    updatedAt: Date,
  };
  partner: {
    id: string,
    displayName: string,
    avatar: string,
  };
}

export interface IConversationRepository
  extends IBaseRepository<IConversation> {
  findByIds(ids: string[]): Promise<IConversation[]>;
  create(data: Partial<IConversation>): Promise<IConversation>;
  updateLastMessage(
    conversationId: string,
    messageContent: string,
    messageTimestamp: Date,
    senderId: string,
  ): Promise<boolean>;
  softDelete(conversationId: string): Promise<boolean>;
  existsConversation(conversationId: string): Promise<boolean>;
  findConversationsPaginated(
    userId: string,
    limit: number,
    cursor?: string,
  ): Promise<ConversationAggregationResult[]>;
  findConversationByIdWithDetails(
    conversationId: string,
    userId: string,
  ): Promise<ConversationAggregationResult | null>;
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
    senderId: string,
  ): Promise<boolean> {
    const result = await this.model.updateOne(
      this.addSoftDeleteFilter({ _id: conversationId }),
      {
        $set: {
          senderId: senderId,
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

  public async findConversationsPaginated(
    userId: string,
    limit: number,
    cursor?: string,
  ): Promise<ConversationAggregationResult[]> {
    const pipeline: PipelineStage[] = [
      {
        $match: {
          deletedAt: null,
          ...(cursor && { lastMessageAt: { $lt: new Date(cursor) } }),
        },
      },
      
      { $sort: { lastMessageAt: -1 } },
      { $limit: limit + 1 },
      {
        $lookup: {
          from: 'conversationparticipants',
          let: { convId: '$_id' },
          pipeline: [
            {
              $match: {
                $expr: {
                  $and: [
                    { $eq: ['$conversationId', '$$convId'] },
                    { $eq: ['$deletedAt', null] },
                  ],
                },
              },
            },
          ],
          as: 'participants',
        },
      },
      {
        $match: {
          'participants.memberId': userId,
        },
      },
      
      {
        $addFields: {
          myParticipant: {
            $first: {
              $filter: {
                input: '$participants',
                cond: { $eq: ['$$this.memberId', userId] },
              },
            },
          },
          partnerParticipant: {
            $first: {
              $filter: {
                input: '$participants',
                cond: { $ne: ['$$this.memberId', userId] },
              },
            },
          },
        },
      },
      {
        $lookup: {
          from: 'users',
          localField: 'partnerParticipant.memberId',
          foreignField: '_id',
          as: 'partnerUser',
        },
      },      
      {
        $project: {
          _id: 1,
          conversationId: '$_id',
          lastMessageContent: 1,
          lastMessageAt: 1,
          senderId: 1,
          myParticipant: {
            unreadCount: 1,
            updatedAt: 1,
          },
          partner: {
            id: '$partnerParticipant.memberId',
            displayName: {
              $ifNull: [
                '$partnerParticipant.partnerDisplayName',
                { $first: '$partnerUser.displayName' },
                'Unknown',
              ],
            },
            avatar: {
              $ifNull: [
                { $first: '$partnerUser.avatarUrl' },
                '',
              ],
            },
          },
        },
      },
    ];

    const result = await this.model.aggregate(pipeline).exec();
    return result as ConversationAggregationResult[];
  }

  public async findConversationByIdWithDetails(
    conversationId: string,
    userId: string,
  ): Promise<ConversationAggregationResult | null> {
    const pipeline: PipelineStage[] = [
      {
        $match: {
          _id: new Types.ObjectId(conversationId),
          deletedAt: null,
        },
      },
      {
        $lookup: {
          from: 'conversationparticipants',
          let: { convId: '$_id' },
          pipeline: [
            {
              $match: {
                $expr: {
                  $and: [
                    { $eq: ['$conversationId', '$$convId'] },
                    { $eq: ['$deletedAt', null] },
                  ],
                },
              },
            },
          ],
          as: 'participants',
        },
      },
      {
        $match: {
          'participants.memberId': userId,
        },
      },      
      {
        $addFields: {
          myParticipant: {
            $first: {
              $filter: {
                input: '$participants',
                cond: { $eq: ['$$this.memberId', userId] },
              },
            },
          },
          partnerParticipant: {
            $first: {
              $filter: {
                input: '$participants',
                cond: { $ne: ['$$this.memberId', userId] },
              },
            },
          },
        },
      },
      {
        $lookup: {
          from: 'users',
          localField: 'partnerParticipant.memberId',
          foreignField: '_id',
          as: 'partnerUser',
        },
      },      
      {
        $project: {
          _id: 1,
          conversationId: '$_id',
          lastMessageContent: 1,
          lastMessageAt: 1,
          senderId: 1,
          myParticipant: {
            unreadCount: 1,
            updatedAt: 1,
          },
          partner: {
            id: '$partnerParticipant.memberId',
            displayName: {
              $ifNull: [
                '$partnerParticipant.partnerDisplayName',
                { $first: '$partnerUser.displayName' },
                'Unknown',
              ],
            },
            avatar: {
              $ifNull: [
                { $first: '$partnerUser.avatarUrl' },
                '',
              ],
            },
          },
        },
      },
    ];

    const result = await this.model
      .aggregate<ConversationAggregationResult>(pipeline)
      .exec();

    return result[0] || null;
  }
  
}