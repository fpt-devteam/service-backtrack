import { Request, Response } from 'express';
import { CreateConversationSchema } from '@/dtos/conversation/conversation.request';
import * as conversationService from '@/services/conversation.service';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';
// import { createError } from '@/utils/api-error';

export const createConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
	
	// if (!userId) {
	// 	throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
	// }

	const parsed = CreateConversationSchema.parse(req.body);
	const conversation = await conversationService.createConversation(parsed, userId);
	const response = ApiResponseBuilder.success({ conversation }, req.headers['x-correlation-id'] as string);
	return res.status(201).json(response);
};

export const listConversations = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	// if (!userId) {
	// 	throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
	// }

	const limit = req.query.limit ? parseInt(req.query.limit as string) : 20;
	const cursor = req.query.cursor as string | undefined;

	const result = await conversationService.listConversationsByUserId(userId, { cursor, limit });
	const response = ApiResponseBuilder.success(result, req.headers['x-correlation-id'] as string);
	return res.status(200).json(response);
};

export const getConversationById = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	// if (!userId) {
	// 	throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
	// }

	const id = req.params.id as string;

	const conversation = await conversationService.getConversationById(id, userId);
	const response = ApiResponseBuilder.success({ conversation }, req.headers['x-correlation-id'] as string);
	return res.status(200).json(response);
};

export const updateConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	// if (!userId) {
	// 	throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
	// }

	const id = req.params.id as string;

	const conversation = await conversationService.updateConversation(id, userId, req.body);
	const response = ApiResponseBuilder.success({ conversation }, req.headers['x-correlation-id'] as string);
	return res.status(200).json(response);
};

export const deleteConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	// if (!userId) {
	// 	throw createError('Unauthorized', 'MISSING_AUTH_HEADER', 'User authentication header is required');
	// }

	const id = req.params.id as string;

	await conversationService.deleteConversation(id, userId);
	const response = ApiResponseBuilder.success({ message: 'Conversation deleted successfully' }, req.headers['x-correlation-id'] as string);
	return res.status(200).json(response);
};