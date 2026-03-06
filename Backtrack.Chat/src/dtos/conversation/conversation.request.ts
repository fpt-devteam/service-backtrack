import { ConversationType, TicketStatus } from "@/models";
import { z } from 'zod';

export const CreateConversationSchema = z
    .discriminatedUnion('type', [
        z.object({
            type: z.literal(ConversationType.ORGANIZATION),
            orgId: z.string(),
        }),
        z.object({
            type: z.literal(ConversationType.PERSONAL),
            memberId: z.string(),
        }),
    ])
    .transform((data) => {
        if (data.type === ConversationType.ORGANIZATION) {
            return { ...data, ticketStatus: TicketStatus.QUEUED };
        }
        return data;
    });

export type CreateConversationRequest = z.output<typeof CreateConversationSchema>;
