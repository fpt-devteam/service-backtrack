import { ConversationType } from "@/models";
import { z } from 'zod';

export const CreateConversationSchema = z.discriminatedUnion('type', [
    z.object({
        type: z.literal(ConversationType.ORGANIZATION),
        orgId: z.string(),
    }),
    z.object({
        type: z.literal(ConversationType.PERSONAL),
        memberId: z.string(),
    }),
]);

export type CreateConversationRequest = z.output<typeof CreateConversationSchema>;
