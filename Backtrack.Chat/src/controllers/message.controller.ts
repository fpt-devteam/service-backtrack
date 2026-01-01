import { Request, Response } from 'express';
import MessageService from '@src/services/message.service';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';
import { ErrorCodes } from '@src/common/errors';
import { getSocketInstance } from '../socket';
import { AsyncHandler } from '@src/decorators/async-handler';
import { HEADER_AUTH_ID } from '@src/utils/headers';
import logger from '@src/utils/logger';

export class MessageController {
  @AsyncHandler
  public async sendMessage(req: Request, res: Response) {
    const senderId = req.headers[HEADER_AUTH_ID] as string;
    const { conversationId } = req.params;
    const { content } = req.body as { content: string };

    if (!senderId) {
      throw ErrorCodes.MissingUserId;
    }

    if (!content) {
      throw ErrorCodes.MissingContent;
    }

    const message = await MessageService.sendMessage(
      senderId,
      conversationId,
      content,
    );

    try {
      const io = getSocketInstance();
      io.to(conversationId).emit('receive_message', message);
    } catch (e) {
      logger.warn('Socket error while emitting receive_message:', e);
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
      throw ErrorCodes.MissingUserId;
    }

    const result = await MessageService.getMessagesByConversationId(
      conversationId,
      userId,
      {
        cursor: cursor as string | undefined,
        limit: limit ? parseInt(limit as string, 10) : undefined,
      },
    );

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: result,
    });
  }
}

export default new MessageController();
