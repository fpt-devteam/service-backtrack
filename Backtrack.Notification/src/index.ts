// MUST be first - load environment variables before anything else
import './configs/env-loader';
import { Server as SocketIOServer } from 'socket.io';

import http from 'http';
import ENV from '@src/common/constants/ENV';
import app from './server';
import { connectDatabase } from './configs/database';
import { startConsumers, stopConsumers } from './messaging/consumer-manager';
import logger from '@src/utils/logger';
import { setSocketInstance } from './socket';

// Connect to database
connectDatabase();

// Start RabbitMQ consumers
startConsumers().catch((error: unknown) => {
  logger.error('Failed to start RabbitMQ consumers:', error);
  logger.warn('Server will continue without message consumers');
});

const httpServer = http.createServer(app);
const io = new SocketIOServer(httpServer, {
  path: '/hub',
  cors: {
    origin: '*',
    methods: ['GET', 'POST'],
    credentials: true,
  },
  transports: ['websocket', 'polling'],
});

setSocketInstance(io);

io.on('connection', (socket) => {
  logger.info(`User connected: ${socket.id}`);
  logger.info(`Transport: ${socket.conn.transport.name}`);
  logger.info(`Client handshake: ${JSON.stringify(socket.handshake.auth)}`);

  socket.on('join_device', (deviceId: string) => {
    if (typeof deviceId !== 'string' || deviceId.trim() === '') {
      logger.warn(`Invalid deviceId from ${socket.id}: ${deviceId}`);
      return;
    }

    socket.join(deviceId);
    logger.info(`Socket ${socket.id} joined device room ${deviceId}`);
  });

  socket.on('leave_device', (deviceId: string) => {
    if (typeof deviceId !== 'string' || deviceId.trim() === '') {
      logger.warn(`Invalid deviceId from ${socket.id}: ${deviceId}`);
      return;
    }

    socket.leave(deviceId);
    logger.info(`Socket ${socket.id} left device room ${deviceId}`);
  });

  socket.on('disconnect', () => {
    logger.info(`User disconnected: ${socket.id}`);
  });

  socket.on('error', (error) => {
    logger.error(`Socket error for ${socket.id}:`, error);
  });
});

// Start server
httpServer.listen(ENV.Port, () => {
  logger.info(`Server started on port ${ENV.Port}`);
});

// Graceful shutdown
process.on('SIGINT', async () => {
  logger.info('Received SIGINT, shutting down gracefully...');
  await stopConsumers();
  httpServer.close(() => {
    logger.info('Server closed');
  });
});

process.on('SIGTERM', async () => {
  logger.info('Received SIGTERM, shutting down gracefully...');
  await stopConsumers();
  httpServer.close(() => {
    logger.info('Server closed');
  });
});
