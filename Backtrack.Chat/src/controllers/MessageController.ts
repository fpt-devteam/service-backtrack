import { Request, Response } from 'express';
import MessageService from '@src/services/MessageService';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';
import { AppError, BadRequestError } from '@src/common/errors';
import { getSocketInstance } from '../socket';
import { AsyncHandler } from '@src/decorators/AsyncHandler';

/**
 * Message controller using class-based approach with @AsyncHandler decorator
 */
class MessageController {
  @AsyncHandler
  public async sendMessage(req: Request, res: Response) {
    const senderId = req.headers['x-user-id'] as string;
    const sendername = req.headers['x-username'] as string;
    const correlationId = req.headers['x-correlation-id'] as string;
    const { conversationId } = req.params;
    const { content } = req.body as { content: string };

    if (!senderId) {
      throw new AppError(
        'MissingUserId',
        'User ID is required in x-user-id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    if (!content) {
      throw new AppError(
        'MissingContent',
        'Content is required',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const message = await MessageService.sendMessage(
      senderId,
      sendername,
      conversationId,
      content,
    );

    // Emit socket event to all clients in the conversation room
    try {
      const io = getSocketInstance();
      io.to(conversationId).emit('receive_message', message);
    } catch (e) {
      // Log socket error but don't fail the request
      // TODO: Replace with proper logger in production
      // logger.error('Socket emission failed:', e);
      console.error('Socket emission failed:', e);
    }

    return res.status(HTTP_STATUS_CODES.Created).json({
      success: true,
      data: message,
      correlationId,
    });
  }

  @AsyncHandler
  public async getMessages(req: Request, res: Response) {
    const userId = req.headers['x-user-id'] as string;
    const correlationId = req.headers['x-correlation-id'] as string;
    const { conversationId } = req.params;
    const { cursor, limit } = req.query;

    if (!userId) {
      throw new AppError(
        'MissingUserId',
        'User ID is required in x-user-id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const result = await MessageService.getMessagesByConversationId(
      conversationId,
      userId,
      cursor as string | undefined,
      limit ? parseInt(limit as string, 10) : undefined,
    );

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: {
        data: result.data,
        pagination: {
          nextCursor: result.nextCursor,
          hasMore: result.hasMore,
        },
      },
      correlationId,
    });
  }
}

// Export singleton instance
export default new MessageController();
