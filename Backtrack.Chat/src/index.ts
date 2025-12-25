// MUST be first - load environment variables before anything else
import './config/env-loader';

import http from 'http';
import { Server as SocketIOServer } from 'socket.io';
import logger from 'jet-logger';
import ENV from '@src/common/constants/ENV';
import app from './server';
import { connectDatabase } from './configs/database';
import { setSocketInstance } from './socket';

// Connect to database
connectDatabase();

// Create HTTP server with Socket.IO
const httpServer = http.createServer(app);
const io = new SocketIOServer(httpServer, {
  cors: { origin: '*' },
});

setSocketInstance(io);

// Socket.IO event handlers
io.on('connection', (socket) => {
  logger.info(`User connected: ${socket.id}`);

  socket.on('join_conversation', (conversationId: string) => {
    if (conversationId?.length > 0) {
      socket.join(conversationId);
      logger.info(`Socket ${socket.id} joined conversation ${conversationId}`);
    } else {
      logger.warn(`Invalid conversation ID: ${conversationId}`);
    }
  });

  socket.on('leave_conversation', (conversationId: string) => {
    if (conversationId?.length > 0) {
      socket.leave(conversationId);
      logger.info(`Socket ${socket.id} left conversation ${conversationId}`);
    }
  });

  socket.on('disconnect', () => {
    logger.info(`User disconnected: ${socket.id}`);
  });
});

// Start server
httpServer.listen(ENV.Port, () => {
  logger.info(`Server started on port ${ENV.Port}`);
});
