import { Request, Response } from 'express';
import { AsyncHandler } from '@src/decorators/AsyncHandler';
import ConversationService from '@src/services/ConversationService';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';
import { AppError } from '@src/common/errors';
import { ConversationType } from '@src/models/Conversation';
import { HEADER_AUTH_ID } from '@src/utils/headers';

class ConversationControllerClass {
  @AsyncHandler
  public async getAllConversations(req: Request, res: Response) {
    const userId = req.headers[HEADER_AUTH_ID] as string;
    const correlationId = req.headers['x-correlation-id'] as string;

    if (!userId) {
      throw new AppError(
        'MissingUserId',
        'User ID is required in X-Auth-Id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const conversations =
      await ConversationService.getAllConversationsByUserId(userId);

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: conversations,
      correlationId,
    });
  }

  @AsyncHandler
  public async getConversationById(req: Request, res: Response) {
    const correlationId = req.headers['x-correlation-id'] as string;
    const { id } = req.params;

    const conversation = await ConversationService.getConversationById(id);

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: conversation,
      correlationId,
    });
  }

  @AsyncHandler
  public async createConversation(req: Request, res: Response) {
    const userId = req.headers[HEADER_AUTH_ID] as string;
    const correlationId = req.headers['x-correlation-id'] as string;
    const { type, participantIds, name } = req.body as {
      type: ConversationType,
      participantIds: string[],
      name?: string,
    };

    if (!userId) {
      throw new AppError(
        'MissingUserInfo',
        'User ID is required in X-Auth-Id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    if (!type || !participantIds || !Array.isArray(participantIds)) {
      throw new AppError(
        'MissingConversationInfo',
        'Type and participantIds are required',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    // Validate input
    if (type === ConversationType.SINGLE && participantIds.length !== 1) {
      throw new AppError(
        'InvalidParticipants',
        'Single conversations must have exactly one participant besides the creator',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    if (type === ConversationType.GROUP && !name) {
      throw new AppError(
        'MissingGroupName',
        'Group conversation must have a name',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const conversation = await ConversationService.createConversation(
      userId,
      type,
      participantIds,
      name,
    );

    return res.status(HTTP_STATUS_CODES.Created).json({
      success: true,
      data: conversation,
      correlationId,
    });
  }
}

export default new ConversationControllerClass();
