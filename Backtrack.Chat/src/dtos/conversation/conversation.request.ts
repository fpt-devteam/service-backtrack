import { ConversationStatus } from '@/models';
import { z } from 'zod';

export const CreationDirectConversationSchema = z.object({
    memberId: z.string().min(1, 'memberId is required'),
});

export type CreationDirectConversationRequest = z.infer<typeof CreationDirectConversationSchema>;

export const CreationOrganizationConversationSchema = z.object({
    orgId: z.string().min(1, 'orgId is required'),
    status: z.enum([ConversationStatus.IN_QUEUE, ConversationStatus.IN_PROGRESS, ConversationStatus.CLOSED]),
});

export type CreationSupportConversationRequest = z.infer<typeof CreationOrganizationConversationSchema>;

export type CreateConversationRequest =
    | CreationDirectConversationRequest
    | CreationSupportConversationRequest;
