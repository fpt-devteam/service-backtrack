import { Server as HttpServer } from 'http';
import { Server as SocketIOServer, Socket } from 'socket.io';
import logger from '@/utils/logger';
import { env, isDevelopment } from '@/config/environment';
import { registerSocketHandlers } from '@/websocket';
import { socketAuthMiddleware } from '@/middlewares/socket-auth.middleware';

let io: SocketIOServer;

export function initializeWebSocket(httpServer: HttpServer): SocketIOServer {
  io = new SocketIOServer(httpServer, {
    path: '/hub',
    cors: {
      // In dev, allow all origins (includes the test UI served from this server)
      origin: isDevelopment ? true : env.CLIENT_URL,
      methods: ['GET', 'POST'],
      credentials: true,
    },
    pingTimeout: 60000,
    pingInterval: 25000,
  });

  // Auth middleware — runs before every connection
  io.use(socketAuthMiddleware);

  io.on('connection', (socket: Socket) => {
    logger.info(`WebSocket client connected: ${socket.id} (user: ${socket.data.userId})`);

    // Each socket joins its own user room so we can push targeted events (e.g. unreadCount)
    if (socket.data.userId) {
      socket.join(`user:${socket.data.userId}`);
    }

    registerSocketHandlers(socket);

    socket.on('disconnect', (reason) => {
      logger.info(`WebSocket client disconnected: ${socket.id}, reason: ${reason}`);
    });

    socket.on('error', (error) => {
      logger.error('WebSocket error:', { socketId: socket.id, error: String(error) });
    });
  });

  logger.info('WebSocket server initialized at path /hub');
  return io;
}

export function getIO(): SocketIOServer {
  if (!io) {
    throw new Error('WebSocket server not initialized. Call initializeWebSocket first.');
  }
  return io;
}