import { Request, Response } from 'express';
import * as messageService from '@/services/message.service';
import { GetMessagesQuerySchema } from '@/dtos/message/message.request';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';
import { createError } from '@/utils/api-error';

export const getMessagesByConversationId = async (req: Request, res: Response) => {
  const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

  if (!userId) {
    throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
  }

  const { conversationId } = req.params;

  const parsed = GetMessagesQuerySchema.parse({
    conversationId,
    userId,
    cursor: req.query.cursor,
    limit: req.query.limit,
  });

  const result = await messageService.getMessagesByConversationId(
    parsed.conversationId,
    parsed.userId,
    { cursor: parsed.cursor, limit: parsed.limit },
  );

  const response = ApiResponseBuilder.success(result);
  return res.status(200).json(response);
};
