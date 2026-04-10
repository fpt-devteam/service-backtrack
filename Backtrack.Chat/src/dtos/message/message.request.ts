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
 * Requires an existing `conversationId` (create conversation via REST endpoint first).
 */
export const SendDirectMessageSchema = MessageBodySchema.extend({
  conversationId: z.string().min(1),
});

export type SendDirectMessageRequest = z.output<typeof SendDirectMessageSchema>;

/**
 * Org / support message  —  WebSocket event: message:send:support
 * Requires an existing `conversationId` (create conversation via REST endpoint first).
 */
export const SendSupportMessageSchema = MessageBodySchema.extend({
  conversationId: z.string().min(1),
});

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
