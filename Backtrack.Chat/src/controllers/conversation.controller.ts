import { Request, Response } from 'express';
import { CreateConversationSchema } from '@/dtos/conversation/conversation.request';
import * as conversationService from '@/services/conversation.service';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';
import { ConversationType } from '@/models';

const getCorrelationId = (req: Request) =>
    req.headers[Constants.HEADERS.CORRELATION_ID] as string;

const parsePaginationParams = (req: Request) => ({
    limit: parseInt(req.query.limit as string) || undefined,
    cursor: req.query.cursor as string | undefined,
});

// Conversation CRUD 

export const createConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const data = CreateConversationSchema.parse(req.body);

    let conversation;
    if (data.type === ConversationType.ORGANIZATION) {
        conversation = await conversationService.createOrgConversation(data, userId);
    } else {
        conversation = await conversationService.createPersonalConversation(data, userId);
    }

    return res.status(201).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const getConversationById = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    const conversation = await conversationService.getConversationById(id, userId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const updateConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    const conversation = await conversationService.updateConversation(id, userId, req.body);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const deleteConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    await conversationService.deleteConversation(id, userId);
    return res.status(200).json(
        ApiResponseBuilder.success({ message: 'Conversation deleted successfully' }, getCorrelationId(req))
    );
};

// Listing 

export const listConversations = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

    const result = await conversationService.listConversationsByUserId(userId, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationQueueByStaff = async (req: Request, res: Response) => {
    const orgId = req.headers[Constants.HEADERS.ORG_ID] as string;

    const result = await conversationService.listConversationsQueueByStaff(orgId, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationAssignedByStaff = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

    const result = await conversationService.listConversationsAssignedByStaff(staffId, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

// Assignment

export const assignStaff = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    const conversation = await conversationService.assignStaff(id, staffId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const unassignStaff = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    await conversationService.queueTicket(id, staffId);
    return res.status(200).json(
        ApiResponseBuilder.success({ message: 'Conversation returned to queue' }, getCorrelationId(req))
    );
};