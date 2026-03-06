import { Socket } from 'socket.io';
import { Constants } from '@/config/constants';
import logger from '@/utils/logger';

/**
 * Socket.IO authentication middleware.
 *
 * Production flow (via Gateway):
 *   Client ──[Bearer JWT]──▶ Gateway (verifyIdToken) ──[x-auth-id header]──▶ Chat Service
 *                                                                                └─ read x-auth-id (trusted)
 *
 * On success sets:
 *   socket.data.userId      — Firebase UID
 *   socket.data.email       — user email (null when injected by gateway)
 *   socket.data.displayName — user display name (null when injected by gateway)
 */
export async function socketAuthMiddleware(
  socket: Socket,
  next: (err?: Error) => void,
): Promise<void> {
  try {
    // ── Priority 1: x-auth-id injected by API Gateway (already verified) ──
    const gatewayUserId = socket.handshake.headers[Constants.HEADERS.AUTH_USER_ID] as string | undefined;
    if (gatewayUserId?.trim()) {
      socket.data.userId      = gatewayUserId.trim();
      socket.data.email       = null;
      socket.data.displayName = null;
      logger.info(`[WS] Authenticated socket ${socket.id} via Gateway → user ${gatewayUserId.trim()}`);
      return next();
    }

    // No x-auth-id header means the request did not come through the API Gateway — reject it.
    logger.warn(`[WS] Missing x-auth-id header — socket ${socket.id} rejected (must connect via API Gateway)`);
    return next(new Error('Unauthorized: missing authentication'));
  } catch (err: any) {
    logger.warn(`[WS] Auth failed for socket ${socket.id}: ${err.message}`);
    next(new Error('Unauthorized: invalid token'));
  }
}

