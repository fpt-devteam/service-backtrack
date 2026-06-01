import { Request, Response } from 'express';
import { CreationDirectConversationSchema, CreationOrganizationConversationSchema } from '@/dtos/conversation/conversation.request';
import * as conversationService from '@/services/conversation.service';
import { ApiResponseBuilder } from '@/utils/api-response';
import { Constants } from '@/config/constants';
import { SupportFormData } from '@/models/interfaces/support-conversation.interface';


const getCorrelationId = (req: Request) =>
    req.headers[Constants.HEADERS.CORRELATION_ID] as string;

const parsePaginationParams = (req: Request) => ({
    limit: parseInt(req.query.limit as string) || undefined,
    cursor: req.query.cursor as string | undefined,
});

// Conversation CRUD

export const createDirectConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const { memberId } = CreationDirectConversationSchema.parse(req.body);

    const conversation = await conversationService.findOrCreateDirectConversation(userId, memberId);

    return res.status(201).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const createOrgConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const { orgId, supportFormData } = CreationOrganizationConversationSchema.parse(req.body);

    const conversation = await conversationService.findOrCreateOrgConversation(userId, orgId, supportFormData ?? {});
    return res.status(201).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const updateSupportFormDataInConversation = async (req: Request, res: Response) => {
	const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
	const conversationId = req.params.id as string;
	const { postId: _ignored, ...supportFormData } = req.body;
	await conversationService.updateConversationSupportFormData(userId, conversationId, supportFormData as Partial<SupportFormData>);
	return res.status(200).json(
		ApiResponseBuilder.success({ message: 'Support form data updated successfully' }, getCorrelationId(req))
	);
};
export const getConversationById = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId = req.headers[Constants.HEADERS.ORG_ID] as string | undefined;
    const id = req.params.id as string;

    const conversation = await conversationService.getConversationById(id, userId, orgId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const getConversationByPartnerId = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const partnerId = req.query.partnerId as string;

    const conversation = await conversationService.findDirectConversationByPartnerId(userId, partnerId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

// updateConversation is not currently supported by the service layer.

export const deleteConversation = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    await conversationService.deleteConversation(id, userId);
    return res.status(200).json(
        ApiResponseBuilder.success({ message: 'Conversation deleted successfully' }, getCorrelationId(req))
    );
};

// Listing

export const listDirectConversations = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

    const result = await conversationService.listConversationsByUserId(userId, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listAllConversations = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;

    const result = await conversationService.listAllConversationsByUserId(userId, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationQueueByStaff = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId  = req.headers[Constants.HEADERS.ORG_ID] as string;
    const isMe   = req.query.isMe === 'true';

    const result = await conversationService.listConversationsQueueByStaff(userId, orgId, isMe, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationAssignedByStaff = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId  = req.headers[Constants.HEADERS.ORG_ID] as string;
    const isMe   = req.query.isMe === 'true';

    const result = await conversationService.listConversationsAssignedByStaff(userId, orgId, isMe, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationResolvedByStaff = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId  = req.headers[Constants.HEADERS.ORG_ID] as string;
    const isMe   = req.query.isMe === 'true';

    const result = await conversationService.listConversationsResolvedByStaff(userId, orgId, isMe, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const listConversationVerifiedByStaff = async (req: Request, res: Response) => {
    const userId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const orgId  = req.headers[Constants.HEADERS.ORG_ID] as string;
    const isMe   = req.query.isMe === 'true';

    const result = await conversationService.listConversationsVerifiedByStaff(userId, orgId, isMe, parsePaginationParams(req));
    return res.status(200).json(
        ApiResponseBuilder.success(result, getCorrelationId(req))
    );
};

export const closeConversationsByPostId = async (req: Request, res: Response) => {
    const postId = req.params.postId as string;

    await conversationService.closeConversationsByPostId(postId);
    return res.status(200).json(
        ApiResponseBuilder.success({ message: 'Conversations closed successfully' }, getCorrelationId(req))
    );
};

export const listConversationsByPostId = async (req: Request, res: Response) => {
    const orgId = req.headers[Constants.HEADERS.ORG_ID] as string;
    const postId = req.params.postId as string;

    const result = await conversationService.listConversationsByPostId(orgId, postId, parsePaginationParams(req));
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

    await conversationService.backToQueue(id, staffId);
    return res.status(200).json(
        ApiResponseBuilder.success({ message: 'Conversation returned to queue' }, getCorrelationId(req))
    );
};

export const verifyConversation = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;
	const postId = (req.body.postId ?? null) as string | null;

    const conversation = await conversationService.markVerified(id, staffId, postId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};

export const resolveConversation = async (req: Request, res: Response) => {
    const staffId = req.headers[Constants.HEADERS.AUTH_USER_ID] as string;
    const id = req.params.id as string;

    const conversation = await conversationService.markResolved(id, staffId);
    return res.status(200).json(
        ApiResponseBuilder.success({ conversation }, getCorrelationId(req))
    );
};
