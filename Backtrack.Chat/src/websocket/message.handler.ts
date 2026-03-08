import { Socket } from 'socket.io';
import logger from '@/utils/logger';
import * as messageService from '@/services/message.service';
import { SendMessageSchema } from '@/dtos/message/message.request';
import { isAppError } from '@/utils/api-error';
import { MessageStatus } from '@/models/message';
import { conversationParticipantService, conversationService } from '@/services';

export function registerMessageHandlers(socket: Socket): void {
  const authUserId = socket.data.userId as string | undefined;

  // Join a conversation room
  socket.on('join:conversation', (conversationId: string) => {
    try {
      socket.join(`conversation:${conversationId}`);
      logger.info(`Socket ${socket.id} joined conversation ${conversationId}`);
      socket.emit('join:conversation:success', { conversationId });
    } catch (error) {
      logger.error('Error joining conversation:', { error: String(error) });
      socket.emit('join:conversation:error', { message: 'Failed to join conversation' });
    }
  });

  // Leave a conversation room
  socket.on('leave:conversation', (conversationId: string) => {
    try {
      socket.leave(`conversation:${conversationId}`);
      logger.info(`Socket ${socket.id} left conversation ${conversationId}`);
      socket.emit('leave:conversation:success', { conversationId });
    } catch (error) {
      logger.error('Error leaving conversation:', { error: String(error) });
    }
  });

  // Send a message
  socket.on('message:send', async (data: unknown) => {
    try {
      const validated = SendMessageSchema.parse({ ...(data as object), senderId: authUserId });
      const message = await messageService.sendMessage(validated);
      const updateMessageConv = {
        lastMessageAt: new Date(),
        lastMessage: {
          content: message.content,
          type: message.type,
        },
      }
      await conversationService.updateConversation(
                  validated.conversationId, validated.senderId, updateMessageConv);
      await conversationParticipantService.updateUnreadCount(
        validated.conversationId, 
        validated.senderId
      );
      
      socket.to(`conversation:${message.conversationId}`).emit('message:new', message);
      socket.emit('message:send:success', message);

      logger.info(`Message sent: ${message.id} in conversation ${message.conversationId}`);
    } catch (error) {
      logger.error('Error sending message:', { error: String(error) });

      if (isAppError(error)) {
        socket.emit('message:send:error', { code: error.code, message: error.message });
      } else {
        socket.emit('message:send:error', { code: 'INTERNAL_ERROR', message: 'Failed to send message' });
      }
    }
  });

  // Mark conversation as read
socket.on('conversation:read', async (data: { conversationId: string }) => {
    try {
      if (!authUserId) return;

      await conversationParticipantService.resetUnreadCount(
        data.conversationId, 
        authUserId
      );

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

  // Typing indicator
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
