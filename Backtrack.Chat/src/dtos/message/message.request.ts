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

/** Shared body fields (type, content, attachments, senderId). */
const MessageBodySchema = z.object({
  senderId: z.string().min(1),
  type: z.nativeEnum(MessageType).default(MessageType.TEXT),
  content: z.string().min(1),
  attachments: z.array(MessageAttachmentSchema).optional(),
});

/**
 * Direct / DM message  —  WebSocket event: message:send
 * Supply either an existing `conversationId` or a `recipientId`
 * (find-or-create semantics).
 */
export const SendDirectMessageSchema = MessageBodySchema.extend({
  conversationId: z.string().min(1).optional(),
  recipientId: z.string().min(1).optional(),
}).refine(
  d => d.conversationId || d.recipientId,
  { message: 'One of conversationId or recipientId is required for a direct message' },
);

export type SendDirectMessageRequest = z.output<typeof SendDirectMessageSchema>;

/**
 * Org / support message  —  WebSocket event: message:send:support
 * Supply either an existing `conversationId` or an `orgId`
 * (find-or-create semantics for the customer's support thread).
 */
export const SendSupportMessageSchema = MessageBodySchema.extend({
  conversationId: z.string().min(1).optional(),
  orgId: z.string().min(1).optional(),
  orgName: z.string().optional(),
  orgSlug: z.string().optional(),
  orgLogoUrl: z.string().optional(),
}).refine(
  d => d.conversationId || d.orgId,
  { message: 'One of conversationId or orgId is required for a support message' },
);

export type SendSupportMessageRequest = z.output<typeof SendSupportMessageSchema>;

/** Resolved payload consumed by message.service — always has conversationId. */
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
