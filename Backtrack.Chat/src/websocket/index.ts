import { Socket } from 'socket.io';
import { registerMessageHandlers } from './message.handler';
import logger from '@/utils/logger';

export function registerSocketHandlers(socket: Socket): void {
  logger.info(`Registering handlers for socket ${socket.id}`);

  // Register message-related handlers
  registerMessageHandlers(socket);

  // Handle disconnection
  socket.on('disconnect', (reason) => {
    logger.info(`Socket ${socket.id} disconnected: ${reason}`);
  });

  // Handle errors
  socket.on('error', (error) => {
    logger.error(`Socket ${socket.id} error:`, { error: String(error) });
  });
}
