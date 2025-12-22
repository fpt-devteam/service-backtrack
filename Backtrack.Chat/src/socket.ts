import { Server as SocketIOServer } from 'socket.io';

let io: SocketIOServer | null = null;

export function setSocketInstance(socketInstance: SocketIOServer) {
  io = socketInstance;
}

export function getSocketInstance(): SocketIOServer {
  if (!io) throw new Error('Socket.io instance not set');
  return io;
}
