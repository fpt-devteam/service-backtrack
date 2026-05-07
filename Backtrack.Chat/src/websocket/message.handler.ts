import { Socket } from 'socket.io';
import logger from '@/utils/logger';
import * as messageService from '@/services/message.service';
import * as conversationService from '@/services/conversation.service';
import { SendDirectMessageSchema, SendSupportMessageSchema } from '@/dtos/message/message.request';
import { isAppError } from '@/utils/api-error';

import { conversationParticipantService } from '@/services';
import ConversationParticipant from '@/models/conversation-participant';
import { getIO } from '@/config/websocket';

// ─── Helpers ──────────────────────────────────────────────────────────────────

const serializeError = (err: unknown): string => {
  if (err instanceof Error) return err.stack ?? err.message;
  try { return JSON.stringify(err); } catch { return String(err); }
};

/**
 * Persist a message, broadcast it to room participants, and optionally
 * notify other sockets about a newly created conversation.
 */
async function persistAndBroadcast(
  socket: Socket,
  authUserId: string,
  conversationId: string,
  payload: {
    type: any;
    content: string;
    attachments?: any;
  },
  isNewRoom: boolean,
  successEvent: string,
): Promise<void> {
  // Auto-join the sender's socket when the room is brand new
  if (isNewRoom) {
    const roomName = `conversation:${conversationId}`;
    if (!socket.rooms.has(roomName)) {
      socket.join(roomName);
      logger.info(`Socket ${socket.id} auto-joined room ${roomName}`);
    }
    await autoJoinOtherParticipants(conversationId, authUserId);
  }

  const { message, unreadUpdates } = await messageService.sendMessage({
    conversationId,
    senderId: authUserId,
    type: payload.type,
    content: payload.content,
    attachments: payload.attachments,
  });

  socket.to(`conversation:${conversationId}`).emit('message:new', message);

  const io = getIO();
  for (const { memberId, unreadCount, lastMessage: last } of unreadUpdates) {
    io.to(`user:${memberId}`).emit('conversation:updated', {
      conversationId,
      unreadCount,
      lastMessage: last,
    });
  }

  socket.emit('conversation:updated', {
    conversationId,
    unreadCount: 0,
    lastMessage: {
      senderId: authUserId,
      content: payload.content,
      timestamp: message.createdAt,
    },
  });

  socket.emit(successEvent, { conversationId, message, isNewConversation: isNewRoom });

  if (isNewRoom) {
    socket.to(`conversation:${conversationId}`).emit('conversation:new', {
      conversationId,
      message,
    });
  }

  logger.info(`Message sent: ${message.id} in conversation ${conversationId}`);
}

// ─── Handler Registration ─────────────────────────────────────────────────────

export function registerMessageHandlers(socket: Socket): void {
  // Read userId fresh from socket.data each time — avoids stale closure on first connect
  const getUserId = (): string | undefined => socket.data.userId as string | undefined;

  // ─── Join / leave org queue room (staff only) ────────────────────────────
  socket.on('join:org:queue', async (data: { orgId: string; limit?: number; cursor?: string }) => {
    const authUserId = getUserId();
    if (!authUserId || !data?.orgId) return;
    const { orgId, limit, cursor } = data;
    try {
      socket.join(`org:${orgId}:queue`);
      const result = await conversationService.listConversationsQueueByStaff(authUserId, orgId, false, { limit, cursor });
      socket.emit('org:queue:list', { orgId, ...result });
      logger.info(`Socket ${socket.id} joined org queue room org:${orgId}:queue`);
    } catch (err) {
      logger.error('Error fetching org queue:', { error: serializeError(err) });
      socket.emit('org:queue:error', { orgId, message: 'Failed to fetch queue' });
    }
  });

  socket.on('leave:org:queue', (orgId: string) => {
    if (!orgId) return;
    socket.leave(`org:${orgId}:queue`);
    logger.info(`Socket ${socket.id} left org queue room org:${orgId}:queue`);
  });

  // ─── Join conversation room ──────────────────────────────────────────────
  socket.on('join:conversation', async (conversationId: string) => {
    const authUserId = getUserId();
    try {
      if (!authUserId) {
        socket.emit('join:conversation:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const participant = await ConversationParticipant.findOne({
        conversationId,
        memberId: authUserId,
        isActive: true,
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
      logger.error('Error joining conversation:', { error: serializeError(error) });
      socket.emit('join:conversation:error', { message: 'Failed to join conversation' });
    }
  });

  // ─── Leave conversation room ─────────────────────────────────────────────
  socket.on('leave:conversation', (conversationId: string) => {
    try {
      socket.leave(`conversation:${conversationId}`);
      logger.info(`Socket ${socket.id} left conversation ${conversationId}`);
      socket.emit('leave:conversation:success', { conversationId });
    } catch (error) {
      logger.error('Error leaving conversation:', { error: serializeError(error) });
    }
  });

  // ─── Send direct / DM message ────────────────────────────────────────────
  socket.on('message:send', async (data: unknown) => {
    const authUserId = getUserId();
    try {
      if (!authUserId) {
        socket.emit('message:send:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const validated = SendDirectMessageSchema.parse({ ...(data as object), senderId: authUserId });
      await persistAndBroadcast(socket, authUserId, validated.conversationId, validated, false, 'message:send:success');
    } catch (error) {
      logger.error('Error sending direct message:', { error: serializeError(error) });
      if (isAppError(error)) {
        socket.emit('message:send:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:error', { code: 'INTERNAL_ERROR', message: 'Failed to send message' });
      }
    }
  });

  // ─── Send org / support message ──────────────────────────────────────────
  socket.on('message:send:support', async (data: unknown) => {
    const authUserId = getUserId();
    try {
      if (!authUserId) {
        socket.emit('message:send:support:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const validated = SendSupportMessageSchema.parse({ ...(data as object), senderId: authUserId });
      await persistAndBroadcast(socket, authUserId, validated.conversationId, validated, false, 'message:send:support:success');
    } catch (error) {
      logger.error('Error sending support message:', { error: serializeError(error) });
      if (isAppError(error)) {
        socket.emit('message:send:support:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:support:error', { code: 'INTERNAL_ERROR', message: 'Failed to send support message' });
      }
    }
  });

  // ─── Mark conversation as read ───────────────────────────────────────────
  socket.on('conversation:read', async (data: { conversationId: string }) => {
    const authUserId = getUserId();
    try {
      if (!authUserId) return;

      await conversationParticipantService.resetUnreadCount(data.conversationId, authUserId);
      await messageService.markMessagesAsSeen(data.conversationId, authUserId);

      socket.to(`conversation:${data.conversationId}`).emit('message:seen', {
        conversationId: data.conversationId,
        readBy: authUserId,
        readAt: new Date(),
      });

      const io = getIO();
      io.to(`user:${authUserId}`).emit('conversation:updated', {
        conversationId: data.conversationId,
        unreadCount: 0,
      });
    } catch (error) {
      logger.error('Error marking conversation as read:', { error: serializeError(error) });
    }
  });

  // ─── Typing indicators ───────────────────────────────────────────────────
  socket.on('typing:start', (data: { conversationId: string; displayName?: string }) => {
    socket.to(`conversation:${data.conversationId}`).emit('typing:user', {
      conversationId: data.conversationId,
      userId: getUserId(),
      displayName: data.displayName,
      isTyping: true,
    });
  });

  socket.on('typing:stop', (data: { conversationId: string }) => {
    socket.to(`conversation:${data.conversationId}`).emit('typing:user', {
      conversationId: data.conversationId,
      userId: getUserId(),
      isTyping: false,
    });
  });
}

// ─── Private utilities ────────────────────────────────────────────────────────

async function autoJoinOtherParticipants(
  conversationId: string,
  excludeUserId: string,
): Promise<void> {
  try {
    const io = getIO();
    const roomName = `conversation:${conversationId}`;

    const participants = await ConversationParticipant.find({
      conversationId,
      memberId: { $ne: excludeUserId },
      isActive: true,
      deletedAt: null,
    }).lean().exec();

    if (!participants.length) return;

    const memberIds = new Set(participants.map(p => p.memberId));
    const allSockets = await io.fetchSockets();

    for (const remoteSocket of allSockets) {
      const socketUserId = remoteSocket.data.userId as string | undefined;
      if (socketUserId && memberIds.has(socketUserId) && !remoteSocket.rooms.has(roomName)) {
        remoteSocket.join(roomName);
        logger.info(`Auto-joined socket ${remoteSocket.id} (user: ${socketUserId}) into room ${roomName}`);
      }
    }
  } catch (err) {
    logger.error('autoJoinOtherParticipants failed:', { conversationId, error: serializeError(err) });
  }
}
