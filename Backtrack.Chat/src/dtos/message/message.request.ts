import { MessageType } from '@/models';
import { z } from 'zod';

const MessageAttachmentSchema = z.object({
  type: z.enum(['image', 'video', 'file']),
  url: z.string().url(),
  fileName: z.string().optional(),
  fileSize: z.number().optional(),
  mimeType: z.string().optional(),
  thumbnail: z.string().url().optional(),
  duration: z.number().optional(),
  width: z.number().optional(),
  height: z.number().optional(),
});

/**
 * Modern send message schema — priority-based conversation resolution:
 *   1. conversationId present → send to existing conversation (classic flow)
 *   2. recipientId present   → find-or-create personal DM
 *   3. orgId present         → find-or-create org support conversation
 *
 * At least one of the three must be provided.
 */
export const SendMessageSchema = z.object({
  senderId: z.string().min(1),
  type: z.nativeEnum(MessageType).default(MessageType.TEXT),
  content: z.string().min(1),
  attachments: z.array(MessageAttachmentSchema).optional(),
  // Conversation resolution — exactly one should be provided
  conversationId: z.string().min(1).optional(),
  recipientId: z.string().min(1).optional(),
  orgId: z.string().min(1).optional(),
}).refine(
  d => d.conversationId || d.recipientId || d.orgId,
  { message: 'One of conversationId, recipientId, or orgId is required' }
);

export type SendMessageRequest = z.output<typeof SendMessageSchema>;

// Resolved payload used by message.service — always has conversationId
export type SendMessagePayload = {
  conversationId: string;
  senderId: string;
  type: MessageType;
  content: string;
  attachments?: z.infer<typeof MessageAttachmentSchema>[];
};

export const GetMessagesQuerySchema = z.object({
  conversationId: z.string().min(1),
  userId: z.string().min(1),
  cursor: z.string().optional(),
  limit: z.coerce.number().int().min(1).max(100).default(20),
});

export type GetMessagesQuery = z.infer<typeof GetMessagesQuerySchema>;
