import { Request, Response } from 'express';
import { CreateConversationSchema } from '@/dtos/conversation/conversation.request';
import * as conversationService from '@/services/conversation.service';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';

export const createConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
	const parsed = CreateConversationSchema.parse(req.body);
	const conversation = await conversationService.createConversation(parsed, userId);
	const response = ApiResponseBuilder.success({ conversation }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(201).json(response);
};

export const listConversations = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	const limit =  parseInt(req.query.limit as string);
	const cursor = req.query.cursor as string | undefined;

	const result = await conversationService.listConversationsByUserId(userId, { cursor, limit });
	const response = ApiResponseBuilder.success(result, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(200).json(response);
};

export const listConversationQueueByStaff = async (req: Request, res: Response) => {
	const orgId = req.headers[Constants.HEADERS.ORG_ID] as string;

	const limit =parseInt(req.query.limit as string);
	const cursor = req.query.cursor as string | undefined;

	const result = await conversationService.listConversationsQueueByStaff( orgId, { cursor, limit });
	const response = ApiResponseBuilder.success(result, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(200).json(response);
}

export const listConversationAssignedByStaff = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
	const orgId = req.headers[Constants.HEADERS.ORG_ID] as string;

	const limit =parseInt(req.query.limit as string);
	const cursor = req.query.cursor as string | undefined;

	const result = await conversationService.listConversationsAssignedByStaff(userId, orgId, { cursor, limit });
	const response = ApiResponseBuilder.success(result, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(200).json(response);
}

export const getConversationById = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	const id = req.params.id as string;

	const conversation = await conversationService.getConversationById(id, userId);
	const response = ApiResponseBuilder.success({ conversation }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(200).json(response);
};

export const updateConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	const id = req.params.id as string;

	const conversation = await conversationService.updateConversation(id, userId, req.body);
	const response = ApiResponseBuilder.success({ conversation }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(204).json(response);
};

export const deleteConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

	const id = req.params.id as string;

	await conversationService.deleteConversation(id, userId);
	const response = ApiResponseBuilder.success({ message: 'Conversation deleted successfully' }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(200).json(response);
};

export const assignStaff = async (req: Request, res: Response) => {
	const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
	const id = req.params.id as string;

	const conversation = await conversationService.assignStaff(id, staffId);
	const response = ApiResponseBuilder.success({ conversation }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(204).json(response);
}

export const unassignStaff = async (req: Request, res: Response) => {
	const id = req.params.id as string;

	const conversation = await conversationService.queueTicket(id);
	const response = ApiResponseBuilder.success({ conversation }, req.headers[Constants.HEADERS.CORRELATION_ID] as string);
	return res.status(204).json(response);
}