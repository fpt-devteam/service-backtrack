
import Conversation, { default as SupportConversation } from "@/models/support-conversation";
import DirectConversation from "@/models/direct-conversation";
import ConversationParticipant from "@/models/conversation-participant";
import ConversationAssignment from "@/models/support-conversation-assignment";
import { ConversationErrors } from "./errors/conversation.errors";
import {
    ConversationPartner,
    ConversationResponse,
    ConversationsListResult,
    DirectConversationResponse,
    DirectConversationsListResult,
    SupportConversationResponse,
    SupportConversationsListResult,
} from '@/dtos/conversation/conversation.response';
import User from "@/models/user.model";
import Org from "@/models/org.model";
import { Constants } from '@/config/constants';
import { buildPaginatedResult, CursorPaginationParams } from '@/utils/pagination';
import { toStringOrNull, ToLeanDoc } from '@/utils/object-id';
import { createOrgConvParticipants, createDirectConvParticipants, unassignConversationParticipant } from './conversation-paticipant.service';
import { ConversationParticipantRole, ConversationStatus, IDirectConversation, ISupportConversation } from '@/models';
import { assignConversation, unassignConversation } from "./conversation-assignment.service";
import { Types } from 'mongoose';

/** Shape of a single row from the aggregation pipeline in list queries */
interface ConversationAggRow {
    conversationId: Types.ObjectId | string;
    type?: string;
    orgId?: string | null;
    orgName?: string | null;
    orgSlug?: string | null;
    orgLogoUrl?: string | null;
    status?: ConversationStatus | null;
    staffAssignId?: string | null;
    lastMessageContent?: string | null;
    lastMessageAt?: Date | null;
    lastMessageSenderId?: string | null;
    unreadCount?: number;
    partner?: {
        id: Types.ObjectId | string;
        displayName: string | null;
        email: string | null;
        avatarUrl: string | null;
    } | null;
    createdAt: Date;
    updatedAt: Date;
}

// export const createDirectConversation = async (
//   data: CreationDirectConversationRequest,
//   userId: string
// ): Promise<IDirectConversation> => {
//   const duplicate = await ConversationParticipant.aggregate([
//     { $match: { memberId: { $in: [data.memberId, userId] }, deletedAt: null } },
//     { $group: { _id: '$conversationId', count: { $sum: 1 } } },
//     { $match: { count: 2 } },
//     { $limit: 1 },
//   ]);

//   if (duplicate.length > 0) {
//     throw ConversationErrors.AlreadyExists;
//   }

//   const conversation = new Conversation();
//   await conversation.save();
//   const conversationId = toStringOrNull(conversation._id);
//   if (!conversationId) {
//     throw ConversationErrors.NotFound;
//   }
//   await createDirectConvParticipants(conversationId, data.memberId, userId);
//   return conversation;
// };

// export const createOrgConversation = async (
//   data: CreationSupportConversationRequest,
//   userId: string
// ): Promise<ISupportConversation> => {
//   // Prevent duplicate: check if an ORGANIZATION conversation with this orgId
//   // already has this user as a CUSTOMER participant
//   const existingConv = await findExistingOrgConversation(userId, data.orgId);
//   if (existingConv) {
//     throw ConversationErrors.AlreadyExists;
//   }

//   const conversation = new Conversation({
//     orgId: data.orgId,
//   });
//   await conversation.save();
//   const conversationId = toStringOrNull(conversation._id);
//   if (!conversationId) {
//     throw ConversationErrors.NotFound;
//   }
//   await createConversationQueue(conversationId);
//   await createOrgConvParticipants(conversationId, ConversationParticipantRole.CUSTOMER, userId);
//   return conversation;
// };

/**
 * Modern flow: find existing Direct conversation between two users,
 * or create a new one if it doesn't exist yet.
 */
export const findOrCreateDirectConversation = async (
  userId: string,
  recipientId: string,
): Promise<DirectConversationResponse> => {
  const duplicate = await ConversationParticipant.aggregate([
    { $match: { memberId: { $in: [userId, recipientId] }, deletedAt: null } },
    { $group: { _id: '$conversationId', count: { $sum: 1 } } },
    { $match: { count: 2 } },
    { $limit: 1 },
  ]);

  const partner = await fetchPartnerUser(recipientId);

  if (duplicate.length > 0) {
    const existing = await DirectConversation.findById(duplicate[0]._id).lean().exec();
    if (existing && !existing.deletedAt) {
      return toDirectConversationResponse(existing, partner);
    }
  }

  const conversation = new DirectConversation();
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) throw ConversationErrors.NotFound;
  await createDirectConvParticipants(conversationId, recipientId, userId);
  return toDirectConversationResponse(conversation.toObject(), partner);
};

/**
 * Find an existing ORGANIZATION conversation where userId is a CUSTOMER participant.
 * Correct approach: query by orgId first, then verify user membership.
 * (The inverse — find by userId first — fails when user has chats with multiple orgs.)
 */
const findExistingOrgConversation = async (
  userId: string,
  orgId: string,
): Promise<ToLeanDoc<ISupportConversation> | null> => {
  const orgConversations = (await Conversation.find({
    orgId,
    status: { $ne: ConversationStatus.CLOSED },
    deletedAt: null,
  }).lean().exec()) as ToLeanDoc<ISupportConversation>[];

  if (!orgConversations.length) return null;

  const convIds = orgConversations.map(c => c._id.toString());

  const participant = await ConversationParticipant.findOne({
    conversationId: { $in: convIds },
    memberId: userId,
    role: ConversationParticipantRole.CUSTOMER,
    deletedAt: null,
  }).lean().exec();

  if (!participant) return null;

  const found = orgConversations.find(
    c => c._id.toString() === participant.conversationId
  ) ?? null;
  return found;
};

/**
 * Find an existing Direct or Support conversation between two parties — read-only, no creation.
 * Returns null if no active conversation exists between the two parties.
 */
export const findDirectConversationByPartnerId = async (
  userId: string,
  partnerId: string,
): Promise<DirectConversationResponse | SupportConversationResponse | null> => {
  const [duplicate, supportConv] = await Promise.all([
    ConversationParticipant.aggregate([
      { $match: { memberId: { $in: [userId, partnerId] }, deletedAt: null } },
      { $group: { _id: '$conversationId', count: { $sum: 1 } } },
      { $match: { count: 2 } },
      { $limit: 1 },
    ]),
    findExistingOrgConversation(userId, partnerId),
  ]);

  if (duplicate.length > 0) {
    const [existing, partner] = await Promise.all([
      DirectConversation.findById(duplicate[0]._id).lean().exec(),
      fetchPartnerUser(partnerId),
    ]);
    if (existing && !existing.deletedAt) {
      return toDirectConversationResponse(existing, partner);
    }
  }

  if (supportConv) {
    return toSupportConversationResponse(supportConv);
  }

  return null;
};

/**
 * Modern flow: find existing org conversation for this user,
 * or create a new one if it doesn't exist yet.
 */
export const findOrCreateOrgConversation = async (
  userId: string,
  orgId: string,
): Promise<SupportConversationResponse> => {
  const existingConv = await findExistingOrgConversation(userId, orgId);
  if (existingConv) return toSupportConversationResponse(existingConv);

  const org = await Org.findById(orgId).lean().exec();
  if (!org) throw ConversationErrors.OrgNotFound;
  const conversation = new Conversation({
    orgId,
    orgName: org.name,
    orgSlug: org.slug,
    orgLogoUrl: org.logoUrl,
    status: ConversationStatus.IN_QUEUE,
  });
  await conversation.save();
  const conversationId = toStringOrNull(conversation._id);
  if (!conversationId) throw ConversationErrors.NotFound;
  await createOrgConvParticipants(conversationId, ConversationParticipantRole.CUSTOMER, userId);
  return toSupportConversationResponse(conversation.toObject());
};


export const getConversationById = async (
    id: string,
    userId: string,
): Promise<DirectConversationResponse | SupportConversationResponse | null> => {
    // ── 1. Resolve conversation type + authorize in parallel ─────────────────
    const [supportConv, directConv, participants] = await Promise.all([
        SupportConversation.findById(id).lean().exec(),
        DirectConversation.findById(id).lean().exec(),
        ConversationParticipant.find({
            conversationId: id,
            isActive: true,
            deletedAt: null,
        }).lean().exec(),
    ]);

    const conv = supportConv ?? directConv;
    if (!conv || conv.deletedAt) return null;

    // ── 2. Authorization ─────────────────────────────────────────────────────
    const myParticipant = participants.find(p => p.memberId === userId);
    if (!myParticipant) throw ConversationErrors.Unauthorized;

    // ── 3. Resolve partner user ──────────────────────────────────────────────
    const otherParticipant = participants.find(p => p.memberId !== userId);

    const partnerUser = otherParticipant?.memberId
        ? await User.findById(otherParticipant.memberId)
              .select('displayName email avatarUrl')
              .lean()
              .exec()
        : null;

    const partner: ConversationPartner | null = partnerUser
        ? {
              id:          partnerUser._id.toString(),
              displayName: partnerUser.displayName ?? null,
              email:       partnerUser.email ?? null,
              avatarUrl:   partnerUser.avatarUrl ?? null,
          }
        : null;

    const lastMessage = conv.lastMessageContent
        ? {
              senderId:  conv.senderId ?? null,
              content:   conv.lastMessageContent,
              timestamp: conv.lastMessageAt ?? null,
          }
        : null;

    const unreadCount = myParticipant.unreadCount ?? 0;

    // ── 4. Discriminate response shape ───────────────────────────────────────
    if (supportConv) {
        const s = supportConv as ToLeanDoc<ISupportConversation>;
        return {
            conversationId:  s._id.toString(),
            orgId:           s.orgId ?? null,
            orgName:         s.orgName ?? null,
            orgSlug:         s.orgSlug ?? null,
            orgLogoUrl:      s.orgLogoUrl ?? null,
            status:          s.status ?? ConversationStatus.IN_QUEUE,
            assignedStaffId: s.staffAssignId ?? null,
            partner,
            lastMessage,
            unreadCount,
            createdAt:  s.createdAt,
            updatedAt:  s.updatedAt,
        } satisfies SupportConversationResponse;
    }

    // directConv is guaranteed non-null here
    const d = directConv as ToLeanDoc<IDirectConversation>;
    return {
        conversationId: d._id.toString(),
        partner,
        lastMessage,
        unreadCount,
        createdAt:  d.createdAt,
        updatedAt:  d.updatedAt,
    } satisfies DirectConversationResponse;
};

/**
 * Fetch a user's public profile as a ConversationPartner shape.
 * Returns null if the user does not exist.
 */
const fetchPartnerUser = async (userId: string): Promise<ConversationPartner | null> => {
    const user = await User.findById(userId)
        .select('displayName email avatarUrl')
        .lean()
        .exec();
    if (!user) return null;
    return {
        id:          user._id.toString(),
        displayName: user.displayName ?? null,
        email:       user.email       ?? null,
        avatarUrl:   user.avatarUrl   ?? null,
    };
};

const toDirectConversationResponse = (doc: ToLeanDoc<IDirectConversation>, partner: ConversationPartner | null = null): DirectConversationResponse => ({
    conversationId: doc._id.toString(),
    partner,
    lastMessage: doc.lastMessageContent
        ? { senderId: doc.senderId ?? null, content: doc.lastMessageContent, timestamp: doc.lastMessageAt ?? null }
        : null,
    unreadCount: 0,
    createdAt: doc.createdAt,
    updatedAt: doc.updatedAt,
});

const toSupportConversationResponse = (doc: ToLeanDoc<ISupportConversation>): SupportConversationResponse => ({
    conversationId: doc._id.toString(),
    orgId: doc.orgId,
    orgName: doc.orgName ?? null,
    orgSlug: doc.orgSlug ?? null,
    orgLogoUrl: doc.orgLogoUrl ?? null,
    status: doc.status ?? ConversationStatus.IN_QUEUE,
    assignedStaffId: doc.staffAssignId ?? null,
    partner: null,     // populated downstream (controller/list query)
    lastMessage: doc.lastMessageContent
        ? { senderId: doc.senderId ?? null, content: doc.lastMessageContent, timestamp: doc.lastMessageAt ?? null }
        : null,
    unreadCount: 0,
    createdAt: doc.createdAt,
    updatedAt: doc.updatedAt,
});

export const assignStaff = async (id: string, staffId: string): Promise<SupportConversationResponse | null> => {
    const conversation = await SupportConversation.findById(id).lean().exec();
    if (!conversation || conversation.deletedAt) throw ConversationErrors.NotFound;

    // Conversation must be waiting in queue to be picked up
    if (conversation.status !== ConversationStatus.IN_QUEUE) throw ConversationErrors.NotInQueue;

    await assignConversation(id, staffId);
    await createOrgConvParticipants(id, ConversationParticipantRole.STAFF, staffId);
    await Conversation.findByIdAndUpdate(id, { staffAssignId: staffId, status: ConversationStatus.IN_PROGRESS });
    // id is guaranteed to be a SupportConversation at this call site
    return getConversationById(id, staffId) as Promise<SupportConversationResponse | null>;
};

export const backToQueue = async (id: string, staffId: string): Promise<boolean> => {
    const conversation = await SupportConversation.findById(id).lean().exec();
    if (!conversation || conversation.deletedAt) throw ConversationErrors.NotFound;
    const assignment = await ConversationAssignment.findOne({
        conversationId: id,
        agentId: staffId,
        unassignedAt: null,
    }).lean().exec();
    if (!assignment) throw ConversationErrors.NotAssigned;

    await unassignConversation(id, staffId);
    await unassignConversationParticipant(id, ConversationParticipantRole.STAFF);
    await Conversation.findByIdAndUpdate(id, { staffAssignId: null, status: ConversationStatus.IN_QUEUE });
    return true;
};
export const deleteConversation = async (id: string, userId: string): Promise<void> => {
    // Check if user is a participant
    const participant = await ConversationParticipant.findOne({
        conversationId: id,
        memberId: userId,
        deletedAt: null
    }).exec();

    if (!participant) {
        throw ConversationErrors.Unauthorized;
    }

    const conversation = await Conversation.findByIdAndUpdate(
        id,
        { deletedAt: new Date() },
        { new: true }
    ).exec();

    if (!conversation) {
        throw ConversationErrors.NotFound;
    }
};
export const lookupPartnerStages = (userId: string) => [
    {
        $lookup: {
            from: 'conversationparticipants',
            localField: 'conversationId',
            foreignField: 'conversationId',
            as: 'allParticipants'
        }
    },
    {
        $addFields: {
            partnerParticipant: {
                $first: {
                    $filter: {
                        input: '$allParticipants',
                        cond: {
                            $and: [
                                { $ne: ['$$this.memberId', userId] },
                                { $eq: ['$$this.isActive', true] },
                                { $eq: ['$$this.deletedAt', null] }
                            ]
                        }
                    }
                }
            }
        }
    },
    {
        $lookup: {
            from: 'users',
            localField: 'partnerParticipant.memberId',
            foreignField: '_id',
            as: 'partnerUser'
        }
    },
    {
        $addFields: { partnerUser: { $first: '$partnerUser' } }
    },
];

export const projectConversationStage = {
    $project: {
        conversationId: '$conversation._id',
        orgId:         { $ifNull: ['$conversation.orgId',         null] },
        orgName:       { $ifNull: ['$conversation.orgName',       null] },
        orgSlug:       { $ifNull: ['$conversation.orgSlug',       null] },
        orgLogoUrl:    { $ifNull: ['$conversation.orgLogoUrl',    null] },
        status:        { $ifNull: ['$conversation.status',        null] },
        staffAssignId: { $ifNull: ['$conversation.staffAssignId', null] },
        lastMessageContent:  '$conversation.lastMessageContent',
        lastMessageAt:       '$conversation.lastMessageAt',
        lastMessageSenderId: '$conversation.senderId',
        unreadCount: { $ifNull: ['$unreadCount', 0] },
        partner: {
            $cond: {
                if: '$partnerUser',
                then: {
                    id:          '$partnerUser._id',
                    displayName: { $ifNull: ['$partnerUser.displayName', null] },
                    email:       { $ifNull: ['$partnerUser.email', null] },
                    avatarUrl:   { $ifNull: ['$partnerUser.avatarUrl', null] },
                },
                else: null
            }
        },
        createdAt: '$conversation.createdAt',
        updatedAt: '$conversation.updatedAt',
    }
};

export const listConversationsByUserId = async (
    userId: string,
    params: CursorPaginationParams = {}
): Promise<DirectConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await ConversationParticipant.aggregate([
        {
            $match: { memberId: userId, isActive: true, deletedAt: null },
        },
        {
            $lookup: {
                from: 'directconversations',
                let: { convIdStr: '$conversationId' },
                pipeline: [
                    {
                        $match: {
                            $expr: {
                                $eq: ['$_id', { $toObjectId: '$$convIdStr' }]
                            }
                        }
                    }
                ],
                as: 'conversation'
            }
        },
        { $unwind: '$conversation' },
        {
            $match: {
                'conversation.deletedAt': null,
                ...(params.cursor && {
                    'conversation.lastMessageAt': { $lt: new Date(params.cursor) }
                })
            }
        },
        {
            $sort: { 'conversation.lastMessageAt': -1 }
        },
        { $limit: limit + 1 },
        ...lookupPartnerStages(userId),
        projectConversationStage,
    ]);

    return formatDirectResult(results, limit);
};

/** Format aggregate results from DirectConversation queries. */
const formatDirectResult = (results: ConversationAggRow[], limit: number): DirectConversationsListResult => {
    const { items, nextCursor, hasMore } = buildPaginatedResult(results, limit, 'lastMessageAt');

    return {
        conversations: items.map((c: ConversationAggRow): DirectConversationResponse => ({
            conversationId: c.conversationId.toString(),
            partner: c.partner
                ? {
                    id: c.partner.id.toString(),
                    displayName: c.partner.displayName,
                    email: c.partner.email,
                    avatarUrl: c.partner.avatarUrl,
                  }
                : null,
            lastMessage: c.lastMessageContent
                ? {
                    senderId: c.lastMessageSenderId ?? null,
                    content: c.lastMessageContent,
                    timestamp: c.lastMessageAt ?? null,
                  }
                : null,
            unreadCount: c.unreadCount ?? 0,
            createdAt: c.createdAt,
            updatedAt: c.updatedAt,
        })),
        nextCursor,
        hasMore,
    };
};

/** Format aggregate results from SupportConversation queries. */
const formatSupportResult = (results: ConversationAggRow[], limit: number): SupportConversationsListResult => {
    const { items, nextCursor, hasMore } = buildPaginatedResult(results, limit, 'lastMessageAt');

    return {
        conversations: items.map((c: ConversationAggRow): SupportConversationResponse => ({
            conversationId: c.conversationId.toString(),
            orgId: c.orgId!,
            orgName: c.orgName ?? null,
            orgSlug: c.orgSlug ?? null,
            orgLogoUrl: c.orgLogoUrl ?? null,
            status: c.status!,
            assignedStaffId: c.staffAssignId ?? null,
            partner: c.partner
                ? {
                    id: c.partner.id.toString(),
                    displayName: c.partner.displayName,
                    email: c.partner.email,
                    avatarUrl: c.partner.avatarUrl,
                  }
                : null,
            lastMessage: c.lastMessageContent
                ? {
                    senderId: c.lastMessageSenderId ?? null,
                    content: c.lastMessageContent,
                    timestamp: c.lastMessageAt ?? null,
                  }
                : null,
            unreadCount: c.unreadCount ?? 0,
            createdAt: c.createdAt,
            updatedAt: c.updatedAt,
        })),
        nextCursor,
        hasMore,
    };
};

/**
 * List conversations in queue for an org.
 * Sorted by lastMessageAt descending.
 */
export const listConversationsQueueByStaff = async (
    orgId: string,
    params: CursorPaginationParams = {}
): Promise<SupportConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                orgId,
                status: ConversationStatus.IN_QUEUE,
                staffAssignId: null,
                deletedAt: null,
                ...(params.cursor && {
                    lastMessageAt: { $lt: new Date(params.cursor) }
                })
            }
        },
        { $sort: { lastMessageAt: -1 } },
        { $limit: limit + 1 },
        { $addFields: { conversationId: '$_id', conversation: '$$ROOT' } },
        ...lookupPartnerStages(orgId),
        projectConversationStage,
    ]);

    return formatSupportResult(results, limit);
};

export const listConversationsAssignedByStaff = async (
    staffId: string,
    params: CursorPaginationParams = {}
): Promise<SupportConversationsListResult> => {
    const limit = Math.min(params.limit || Constants.PAGINATION.DEFAULT_LIMIT, Constants.PAGINATION.MAX_LIMIT);

    const results = await Conversation.aggregate([
        {
            $match: {
                staffAssignId: staffId,
                status: ConversationStatus.IN_PROGRESS,
                deletedAt: null,
                ...(params.cursor && {
                    lastMessageAt: { $lt: new Date(params.cursor) }
                })
            }
        },
        { $sort: { lastMessageAt: -1 } },
        { $limit: limit + 1 },
        { $addFields: { conversationId: '$_id', conversation: '$$ROOT' } },
        ...lookupPartnerStages(staffId),
        projectConversationStage,
    ]);

    return formatSupportResult(results, limit);
};

// ─── Mixed list (Direct + Support) ───────────────────────────────────────────

/** Internal row shape coming out of the mixed aggregate */
interface MixedConversationAggRow {
    conversationId:      string;
    type:                string;             // 'direct' | 'support'
    orgId:               string | null;
    orgName:             string | null;
    orgSlug:             string | null;
    orgLogoUrl:          string | null;
    status:              ConversationStatus | null;
    staffAssignId:       string | null;
    lastMessageAt:       Date | null;
    lastMessageContent:  string | null;
    lastMessageSenderId: string | null;
    unreadCount:         number;
    partner: {
        id:          Types.ObjectId | string;
        displayName: string | null;
        email:       string | null;
        avatarUrl:   string | null;
    } | null;
    createdAt: Date;
    updatedAt: Date;
}

/** Standard partner projection from a user-doc alias inside aggregation */
const partnerExpr = (alias: string) => ({
    $cond: {
        if:   `$${alias}`,
        then: {
            id:          `$${alias}._id`,
            displayName: { $ifNull: [`$${alias}.displayName`, null] },
            email:       { $ifNull: [`$${alias}.email`,       null] },
            avatarUrl:   { $ifNull: [`$${alias}.avatarUrl`,   null] },
        },
        else: null,
    },
});

/**
 * Partner lookup stages for branches where the conversation is stored
 * under the `conv` alias (i.e. the mixed-list aggregate branches).
 * Uses a sub-pipeline so the join key stays as a string field.
 */
const lookupPartnerInBranch = (currentUserId: string) => [
    {
        $lookup: {
            from: 'conversationparticipants',
            let:  { cid: '$conversationId' },
            pipeline: [
                {
                    $match: {
                        $expr:     { $eq: ['$conversationId', '$$cid'] },
                        isActive:  true,
                        deletedAt: null,
                        memberId:  { $ne: currentUserId },
                    },
                },
                { $limit: 1 },
            ],
            as: 'partnerParticipant',
        },
    },
    { $addFields: { partnerParticipant: { $first: '$partnerParticipant' } } },
    // User._id is a String (Firebase UID) — join directly, no $toObjectId needed
    {
        $lookup: {
            from:         'users',
            localField:   'partnerParticipant.memberId',
            foreignField: '_id',
            as:           'partnerUser',
        },
    },
    { $addFields: { partnerUser: { $first: '$partnerUser' } } },
];

/**
 * Build one branch of the mixed-list aggregate.
 * @param collection  Mongoose collection name ('directconversations' | 'supportconversations')
 * @param type        Literal tag embedded in each result row
 * @param userId      The caller's id (excluded from partner lookup)
 * @param cursorFilter Optional $match stage for cursor pagination
 * @param extraProject Additional $project fields specific to the conversation type
 */
const buildConvBranch = (
    collection: string,
    type: 'direct' | 'support',
    userId: string,
    cursorFilter: Record<string, unknown>,
    extraProject: Record<string, unknown>,
) =>
    ConversationParticipant.aggregate<MixedConversationAggRow>([
        { $match: { memberId: userId, isActive: true, deletedAt: null } },
        {
            $lookup: {
                from: collection,
                let:  { cid: '$conversationId' },
                pipeline: [{
                    $match: {
                        $expr:     { $eq: ['$_id', { $toObjectId: '$$cid' }] },
                        deletedAt: null,
                    },
                }],
                as: 'conv',
            },
        },
        { $unwind: '$conv' },
        ...(Object.keys(cursorFilter).length ? [{ $match: cursorFilter }] : []),
        ...lookupPartnerInBranch(userId),
        {
            $project: {
                _id:                 0,
                conversationId:      { $toString: '$conv._id' },
                type:                { $literal: type },
                lastMessageAt:       '$conv.lastMessageAt',
                lastMessageContent:  '$conv.lastMessageContent',
                lastMessageSenderId: '$conv.senderId',
                unreadCount:         { $ifNull: ['$unreadCount', 0] },
                partner:             partnerExpr('partnerUser'),
                createdAt:           '$conv.createdAt',
                updatedAt:           '$conv.updatedAt',
                ...extraProject,
            },
        },
    ]);

/**
 * Return every conversation (Direct + Support) that `userId` participates in,
 * sorted by lastMessageAt desc and cursor-paginated.
 *
 * GET /conversations
 */
export const listAllConversationsByUserId = async (
    userId: string,
    params: CursorPaginationParams = {}
): Promise<ConversationsListResult> => {
    const limit = Math.min(
        params.limit ?? Constants.PAGINATION.DEFAULT_LIMIT,
        Constants.PAGINATION.MAX_LIMIT
    );

    const cursorFilter = params.cursor
        ? { 'conv.lastMessageAt': { $lt: new Date(params.cursor) } }
        : {};

    const [directBranch, supportBranch] = await Promise.all([
        buildConvBranch('directconversations', 'direct', userId, cursorFilter, {
            orgId:         { $literal: null },
            orgName:       { $literal: null },
            orgSlug:       { $literal: null },
            orgLogoUrl:    { $literal: null },
            status:        { $literal: null },
            staffAssignId: { $literal: null },
        }),
        buildConvBranch('supportconversations', 'support', userId, cursorFilter, {
            orgId:         { $ifNull: ['$conv.orgId',         null] },
            orgName:       { $ifNull: ['$conv.orgName',       null] },
            orgSlug:       { $ifNull: ['$conv.orgSlug',       null] },
            orgLogoUrl:    { $ifNull: ['$conv.orgLogoUrl',    null] },
            status:        { $ifNull: ['$conv.status',        null] },
            staffAssignId: { $ifNull: ['$conv.staffAssignId', null] },
        }),
    ]);

    // ── Merge → sort → paginate in-process ─────────────────────────────────
    const merged = [...directBranch, ...supportBranch].sort((a, b) => {
        const ta = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
        const tb = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
        return tb - ta;                          // newest first
    });

    const hasMore  = merged.length > limit;
    const items    = merged.slice(0, limit);
    const lastItem = items[items.length - 1];
    const nextCursor = hasMore && lastItem?.lastMessageAt
        ? new Date(lastItem.lastMessageAt).toISOString()
        : null;

    return {
        conversations: items.map((c): ConversationResponse => ({
            conversationId:  c.conversationId,
            type:            c.type,
            orgId:           c.orgId,
            orgName:         c.orgName ?? null,
            orgSlug:         c.orgSlug ?? null,
            orgLogoUrl:      c.orgLogoUrl ?? null,
            status:          c.status,
            assignedStaffId: c.staffAssignId,
            partner: c.partner
                ? {
                    id:          c.partner.id.toString(),
                    displayName: c.partner.displayName,
                    email:       c.partner.email,
                    avatarUrl:   c.partner.avatarUrl,
                  }
                : null,
            lastMessage: c.lastMessageContent
                ? {
                    senderId:  c.lastMessageSenderId ?? null,
                    content:   c.lastMessageContent,
                    timestamp: c.lastMessageAt ?? null,
                  }
                : null,
            unreadCount: c.unreadCount ?? 0,
            createdAt:   c.createdAt,
            updatedAt:   c.updatedAt,
        })),
        nextCursor,
        hasMore,
    };
};
