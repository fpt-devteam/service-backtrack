import Message from '@/models/message';
import { MessageStatus } from '@/models';
import SupportConversation from '@/models/support-conversation';
import DirectConversation from '@/models/direct-conversation';
import ConversationParticipant from '@/models/conversation-participant';
import { SendMessagePayload } from '@/dtos/message/message.request';
import { MessageResponse, MessagesResponse } from '@/dtos/message/message.response';
import { CursorPaginationParams, cursorPaginate } from '@/utils/pagination';
import { ConversationErrors } from './errors/conversation.errors';
import logger from '@/utils/logger';

/**
 * Find a conversation by ID across both DirectConversation and SupportConversation models.
 * Returns { doc, model } so the caller can update the correct collection.
 */
const findConversationById = async (conversationId: string) => {
  const direct = await DirectConversation.findById(conversationId).exec();
  if (direct && !direct.deletedAt) return { doc: direct, model: DirectConversation };

  const support = await SupportConversation.findById(conversationId).exec();
  if (support && !support.deletedAt) return { doc: support, model: SupportConversation };

  return null;
};

export const sendMessage = async (data: SendMessagePayload): Promise<MessageResponse> => {
  // Verify conversation exists (direct or support)
  const found = await findConversationById(data.conversationId);
  if (!found) {
    throw ConversationErrors.NotFound;
  }

  // Verify sender is a participant
  const participant = await ConversationParticipant.findOne({
    conversationId: data.conversationId,
    memberId: data.senderId,
    deletedAt: null,
  }).exec();

  if (!participant) {
    throw ConversationErrors.Unauthorized;
  }

  // Create message
  const message = new Message({
    conversationId: data.conversationId,
    senderId: data.senderId,
    type: data.type,
    content: data.content,
    attachments: data.attachments || [],
    status: MessageStatus.SENT,
  });

  await message.save();

  // Update conversation last message using the correct model
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  await (found.model as any).findByIdAndUpdate(data.conversationId, {
    lastMessageContent: data.content,
    lastMessageAt: message.createdAt,
    senderId: data.senderId,
  }).exec();

  // Increment unread count for other participants
  await ConversationParticipant.updateMany(
    {
      conversationId: data.conversationId,
      memberId: { $ne: data.senderId },
      deletedAt: null,
    },
    {
      $inc: { unreadCount: 1 },
    }
  ).exec();

  logger.info(`Message sent in conversation ${data.conversationId} by ${data.senderId}`);

  return {
    id: message._id.toString(),
    conversationId: message.conversationId.toString(),
    senderId: message.senderId,
    type: message.type,
    content: message.content,
    attachments: message.attachments ?? undefined,
    status: message.status!,
    createdAt: message.createdAt,
    updatedAt: message.updatedAt,
  };
};

export const getMessagesByConversationId = async (
  conversationId: string,
  userId: string,
  params: CursorPaginationParams
): Promise<MessagesResponse> => {
  // Verify conversation exists (direct or support)
  const found = await findConversationById(conversationId);
  if (!found) {
    throw ConversationErrors.NotFound;
  }

  // Verify user is a participant
  const participant = await ConversationParticipant.findOne({
    conversationId,
    memberId: userId,
    deletedAt: null,
  }).exec();

  if (!participant) {
    throw ConversationErrors.Unauthorized;
  }

  // Get paginated messages
  const result = await cursorPaginate(
    Message as any,
    { conversationId, deletedAt: null },
    params,
    'createdAt',
    -1 // Newest first
  );

  const messages: MessageResponse[] = result.items.map((msg: any) => ({
    id: msg._id.toString(),
    conversationId: msg.conversationId.toString(),
    senderId: msg.senderId,
    type: msg.type,
    content: msg.content,
    attachments: msg.attachments,
    status: msg.status!,
    createdAt: msg.createdAt,
    updatedAt: msg.updatedAt,
  }));

  return {
    messages,
    nextCursor: result.nextCursor,
    hasMore: result.hasMore,
  };
};

export const updateMessageStatus = async (messageId: string, status: MessageStatus): Promise<void> => {
  await Message.findByIdAndUpdate(messageId, { status }).exec();
};
export const markMessagesAsSeen = async (conversationId: string, userId: string): Promise<void> => {
  await Message.updateMany(
    { 
      conversationId, 
      senderId: { $ne: userId },      
      status: { $ne: MessageStatus.SEEN } 
    },
    { $set: { status: MessageStatus.SEEN } }
  );
};