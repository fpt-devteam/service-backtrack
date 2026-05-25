import { z } from 'zod';

export const CreationDirectConversationSchema = z.object({
    memberId: z.string().min(1, 'memberId is required'),
});

export type CreationDirectConversationRequest = z.infer<typeof CreationDirectConversationSchema>;

export const CreationOrganizationConversationSchema = z.object({
    orgId: z.string().min(1, 'orgId is required'),
	supportFormData: z.object({
		postId: z.string().nullish(),
		category: z.string(),
		subCategoryId: z.string(),
		itemName: z.string(),
		color: z.string(),
		additionalDetails: z.string().nullish(),
		imageUrls: z.array(z.string()).nullish(),
		lostLocation: z.string().nullish(),
		eventTime: z.coerce.date().nullish(),
		contactName: z.string(),
		contactPhone: z.string(),
		contactEmail: z.string().email(),
	}).partial(),
    // status: z.enum([ConversationStatus.IN_QUEUE, ConversationStatus.IN_PROGRESS, ConversationStatus.CLOSED]),
});

export type CreationSupportConversationRequest = z.infer<typeof CreationOrganizationConversationSchema>;

export type CreateConversationRequest =
    | CreationDirectConversationRequest
    | CreationSupportConversationRequest;
