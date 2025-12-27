import { Request, Response } from 'express';
import MessageService from '@src/services/message.service';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';
import { AppError } from '@src/common/errors';
import { getSocketInstance } from '../socket';
import { AsyncHandler } from '@src/decorators/async-handler';
import { HEADER_AUTH_ID } from '@src/utils/headers';

/**
 * Message controller using class-based approach with @AsyncHandler decorator
 */
export class MessageController {
  @AsyncHandler
  public async sendMessage(req: Request, res: Response) {
    const senderId = req.headers[HEADER_AUTH_ID] as string;
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
      // sendername,
      conversationId,
      content,
    );

    // Emit socket event to all clients in the conversation room
    try {
      const io = getSocketInstance();
      io.to(conversationId).emit('receive_message', message);
    } catch (e) {
      // Socket error logged but doesn't fail the request
    }

    return res.status(HTTP_STATUS_CODES.Created).json({
      success: true,
      data: message,
    });
  }

  @AsyncHandler
  public async getMessages(req: Request, res: Response) {
    const userId = req.headers[HEADER_AUTH_ID] as string;
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
    });
  }
}

export default new MessageController();
