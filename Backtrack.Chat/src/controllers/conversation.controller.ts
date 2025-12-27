import { Request, Response } from 'express';
import { AsyncHandler } from '@src/decorators/async-handler';
import ConversationService from '@src/services/conversation.service';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';
import { AppError } from '@src/common/errors';
import { HEADER_AUTH_ID } from '@src/utils/headers';
import { 
  CreateConversationInput,
} from '@src/contracts/requests/conversation.request';

export class ConversationControllerClass {
  @AsyncHandler
  public async getAllConversations(req: Request, res: Response) {
    const userId = req.headers[HEADER_AUTH_ID] as string;
    const correlationId = req.headers['x-correlation-id'] as string;
    const { limit, cursor } = req.query;

    if (!userId) {
      throw new AppError(
        'MissingUserId',
        'User ID is required in X-Auth-Id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const paginationOptions = {
      limit: limit ? parseInt(limit as string, 10) : undefined,
      cursor: cursor as string | undefined,
    };

    const conversations =
      await ConversationService.getAllConversationsByUserId(
        userId,
        paginationOptions,
      );

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: conversations,
      correlationId,
    });
  }

  @AsyncHandler
  public async getConversationById(req: Request, res: Response) {
    const { id } = req.params;

    const conversation = await ConversationService.getConversationById(id);

    return res.status(HTTP_STATUS_CODES.Ok).json({
      success: true,
      data: conversation,
    });
  }

  @AsyncHandler
  public async createConversation(req: Request, res: Response) {
    const userId = req.headers[HEADER_AUTH_ID] as string;
    const {
      partnerId,
      creatorKeyName,
      partnerKeyName,
      customAvatarUrl,
    } = req.body as Omit<CreateConversationInput, 'creatorId'>;

    if (!userId) {
      throw new AppError(
        'MissingUserInfo',
        'User ID is required in X-Auth-Id header',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    if (!partnerId) {
      throw new AppError(
        'MissingPartnerInfo',
        'partnerId is required',
        HTTP_STATUS_CODES.BadRequest,
      );
    }

    const conversationInput: CreateConversationInput = {
      creatorId: userId,
      partnerId,
      creatorKeyName,
      partnerKeyName,
      customAvatarUrl,
    };

    const conversationId = await ConversationService.createConversation(
      conversationInput,
    );

    return res.status(HTTP_STATUS_CODES.Created).json({
      success: true,
      data: { conversationId },
    });
  }
}

export default new ConversationControllerClass();
