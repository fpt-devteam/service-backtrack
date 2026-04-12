import { Socket } from 'socket.io';
import logger from '@/utils/logger';
import * as messageService from '@/services/message.service';
import { SendDirectMessageSchema, SendSupportMessageSchema } from '@/dtos/message/message.request';
import { isAppError } from '@/utils/api-error';

import { conversationParticipantService } from '@/services';
import ConversationParticipant from '@/models/conversation-participant';
import { getIO } from '@/config/websocket';

// ─── Helpers ──────────────────────────────────────────────────────────────────

/**
 * Persist a message, broadcast it to room participants, and optionally
 * notify other sockets about a newly created conversation.
 *
 * Extracted so that both the direct and support handlers share
 * identical post-resolve behaviour without code duplication.
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
    // Pull all other participants' open sockets into the same room
    await autoJoinOtherParticipants(conversationId, authUserId);
  }

  // Persist (also updates lastMessage + increments unreadCount for others)
  const { message, unreadUpdates } = await messageService.sendMessage({
    conversationId,
    senderId: authUserId,
    type: payload.type,
    content: payload.content,
    attachments: payload.attachments,
  });

  // Broadcast to everyone else in the room
  socket.to(`conversation:${conversationId}`).emit('message:new', message);

  // Push unreadCount + lastMessage to each participant's user room
  // This allows the conversation list to update without being in the conversation room
  const io = getIO();
  for (const { memberId, unreadCount, lastMessage: last } of unreadUpdates) {
    io.to(`user:${memberId}`).emit('conversation:updated', {
      conversationId,
      unreadCount,
      lastMessage: last,
    });
  }

  // Also notify the sender so their own conversation list updates lastMessage
  socket.emit('conversation:updated', {
    conversationId,
    unreadCount: 0,
    lastMessage: {
      senderId: authUserId,
      content: payload.content,
      timestamp: message.createdAt,
    },
  });

  // Acknowledge to the sender
  socket.emit(successEvent, { conversationId, message, isNewConversation: isNewRoom });

  // If this is the very first message, tell other in-room participants
  // about the new conversation (after message:new so client has both events in order)
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
  const authUserId = socket.data.userId as string | undefined;

  // ─── Join conversation room ──────────────────────────────────────────────
  socket.on('join:conversation', async (conversationId: string) => {
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
      logger.error('Error joining conversation:', { error: String(error) });
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
      logger.error('Error leaving conversation:', { error: String(error) });
    }
  });

  // ─── Send direct / DM message ────────────────────────────────────────────
  //
  // Event: message:send
  // Payload: { conversationId, content, type?, attachments? }
  //
  // Conversation must be created beforehand via REST POST /conversations/direct.
  socket.on('message:send', async (data: unknown) => {
    try {
      if (!authUserId) {
        socket.emit('message:send:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const validated = SendDirectMessageSchema.parse({ ...(data as object), senderId: authUserId });

      await persistAndBroadcast(socket, authUserId, validated.conversationId, validated, false, 'message:send:success');
    } catch (error) {
      logger.error('Error sending direct message:', { error: String(error) });
      if (isAppError(error)) {
        socket.emit('message:send:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:error', { code: 'INTERNAL_ERROR', message: 'Failed to send message' });
      }
    }
  });

  // ─── Send org / support message ──────────────────────────────────────────
  //
  // Event: message:send:support
  // Payload: { conversationId, content, type?, attachments? }
  //
  // Conversation must be created beforehand via REST POST /conversations/org.
  socket.on('message:send:support', async (data: unknown) => {
    try {
      if (!authUserId) {
        socket.emit('message:send:support:error', { code: 'UNAUTHORIZED', message: 'User not authenticated' });
        return;
      }

      const validated = SendSupportMessageSchema.parse({ ...(data as object), senderId: authUserId });

      await persistAndBroadcast(socket, authUserId, validated.conversationId, validated, false, 'message:send:support:success');
    } catch (error) {
      logger.error('Error sending support message:', { error: String(error) });
      if (isAppError(error)) {
        socket.emit('message:send:support:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:support:error', { code: 'INTERNAL_ERROR', message: 'Failed to send support message' });
      }
    }
  });

  // ─── Mark conversation as read ───────────────────────────────────────────
  socket.on('conversation:read', async (data: { conversationId: string }) => {
    try {
      if (!authUserId) return;

      await conversationParticipantService.resetUnreadCount(data.conversationId, authUserId);
      await messageService.markMessagesAsSeen(data.conversationId, authUserId);

      // Notify other participants that this user has read the conversation
      socket.to(`conversation:${data.conversationId}`).emit('message:seen', {
        conversationId: data.conversationId,
        readBy: authUserId,
        readAt: new Date(),
      });

      // Push unreadCount = 0 to the reader's own user room (sync other tabs/devices)
      const io = getIO();
      io.to(`user:${authUserId}`).emit('conversation:updated', {
        conversationId: data.conversationId,
        unreadCount: 0,
      });
    } catch (error) {
      logger.error('Error marking conversation as read:', { error: String(error) });
    }
  });

  // ─── Typing indicators ───────────────────────────────────────────────────
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

// ─── Private utilities ────────────────────────────────────────────────────────

/**
 * Find all connected sockets belonging to other ACTIVE participants of a
 * conversation and auto-join them into the room so they receive the very
 * first message in real-time without needing to call join:conversation.
 */
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
    // Non-fatal: message is already persisted; recipient won't get real-time push this round.
    logger.error('autoJoinOtherParticipants failed:', { conversationId, error: String(err) });
  }
}
