import { Socket } from 'socket.io';
import logger from '@/utils/logger';
import * as messageService from '@/services/message.service';
import { SendMessageSchema } from '@/dtos/message/message.request';
import { isAppError } from '@/utils/api-error';
import { conversationParticipantService, conversationService } from '@/services';
import ConversationParticipant from '@/models/conversation-participant';
import { IConversation } from '@/models';

/**
 * Extract the string ID from either a Mongoose Document (has virtual .id getter)
 * or a plain lean object (only has ._id as ObjectId / string).
 * Prevents silent undefined when findOrCreate returns a lean result.
 */
function extractId(conv: IConversation): string {
  return conv.id ?? (conv as any)._id?.toString() ?? '';
}

export function registerMessageHandlers(socket: Socket): void {
  const authUserId = socket.data.userId as string | undefined;

  // ─── Join conversation room ────────────────────────────────────────────────
  // Verify the requesting user is actually a participant before subscribing.
  socket.on('join:conversation', async (conversationId: string) => {
    try {
      if (!authUserId) {
        socket.emit('join:conversation:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const participant = await ConversationParticipant.findOne({
        conversationId,
        memberId: authUserId,
        deletedAt: null,
      }).lean().exec();

      if (!participant) {
        socket.emit('join:conversation:error', { code: 'FORBIDDEN', message: 'Not a participant of this conversation' });
        return;
      }

      socket.join(`conversation:${conversationId}`);
      logger.info(`Socket ${socket.id} joined conversation ${conversationId}`);
      socket.emit('join:conversation:success', { conversationId });
    } catch (error) {
      logger.error('Error joining conversation:', { error: String(error) });
      socket.emit('join:conversation:error', { message: 'Failed to join conversation' });
    }
  });

  // ─── Leave conversation room ───────────────────────────────────────────────
  socket.on('leave:conversation', (conversationId: string) => {
    try {
      socket.leave(`conversation:${conversationId}`);
      logger.info(`Socket ${socket.id} left conversation ${conversationId}`);
      socket.emit('leave:conversation:success', { conversationId });
    } catch (error) {
      logger.error('Error leaving conversation:', { error: String(error) });
    }
  });

  // ─── Send message (modern find-or-create flow) ────────────────────────────
  //
  // Priority for resolving which conversation to send to:
  //   1. conversationId present → send to that existing conversation (classic)
  //   2. recipientId present   → find-or-create personal DM (first message creates conv)
  //   3. orgId present         → find-or-create org conversation (first message creates conv)
  //
  // The findOrCreate functions guarantee idempotency: calling twice with the
  // same pair returns the same conversation, never creates a duplicate.
  socket.on('message:send', async (data: unknown) => {
    try {
      if (!authUserId) {
        socket.emit('message:send:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const validated = SendMessageSchema.parse({ ...(data as object), senderId: authUserId });

      // Step 1: Resolve conversationId
      let conversationId: string;
      let isNewRoom = false;

      if (validated.conversationId) {
        conversationId = validated.conversationId;

      } else if (validated.recipientId) {
        const conv = await conversationService.findOrCreatePersonalConversation(
          authUserId,
          validated.recipientId,
        );
        conversationId = extractId(conv);
        isNewRoom = true;

      } else {
        const conv = await conversationService.findOrCreateOrgConversation(
          authUserId,
          validated.orgId!,
        );
        conversationId = extractId(conv);
        isNewRoom = true;
      }

      // Guard: if extractId fails for any reason, abort cleanly
      if (!conversationId) {
        socket.emit('message:send:error', { code: 'INTERNAL_ERROR', message: 'Failed to resolve conversation' });
        return;
      }

      // Auto-join: server subscribes the socket to the room immediately after
      // resolve/create so the client receives message:send:success already in-room.
      // No client round-trip (join:conversation event) is needed for the new flow.
      if (isNewRoom) {
        const roomName = `conversation:${conversationId}`;
        if (!socket.rooms.has(roomName)) {
          socket.join(roomName);
          logger.info(`Socket ${socket.id} auto-joined room ${roomName}`);
        }
      }

      // Step 2: Persist message (handles lastMessage + unreadCount internally)
      const message = await messageService.sendMessage({
        conversationId,
        senderId: authUserId,
        type: validated.type,
        content: validated.content,
        attachments: validated.attachments,
      });

      // Step 3: Broadcast
      socket.to(`conversation:${conversationId}`).emit('message:new', message);
      // isNewConversation flag lets the client update room list without a join round-trip
      socket.emit('message:send:success', { conversationId, message, isNewConversation: isNewRoom });

      logger.info(`Message sent: ${message.id} in conversation ${conversationId}`);
    } catch (error) {
      logger.error('Error sending message:', { error: String(error) });
      if (isAppError(error)) {
        socket.emit('message:send:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:error', { code: 'INTERNAL_ERROR', message: 'Failed to send message' });
      }
    }
  });

  // ─── Mark conversation as read ────────────────────────────────────────────
  socket.on('conversation:read', async (data: { conversationId: string }) => {
    try {
      if (!authUserId) return;

      await conversationParticipantService.resetUnreadCount(data.conversationId, authUserId);
      await messageService.markMessagesAsSeen(data.conversationId, authUserId);

      socket.to(`conversation:${data.conversationId}`).emit('message:seen', {
        conversationId: data.conversationId,
        readBy: authUserId,
        readAt: new Date(),
      });
    } catch (error) {
      logger.error('Error marking conversation as read:', { error: String(error) });
    }
  });

  // ─── Typing indicators ────────────────────────────────────────────────────
  socket.on('typing:start', (data: { conversationId: string; displayName?: string }) => {
    socket.to(`conversation:${data.conversationId}`).emit('typing:user', {
      conversationId: data.conversationId,
      userId: authUserId,
      displayName: data.displayName,
      isTyping: true,
    });
  });

  socket.on('typing:stop', (data: { conversationId: string }) => {
    socket.to(`conversation:${data.conversationId}`).emit('typing:user', {
      conversationId: data.conversationId,
      userId: authUserId,
      isTyping: false,
    });
  });
}
