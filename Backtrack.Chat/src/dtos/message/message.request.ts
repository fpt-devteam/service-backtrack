import { z } from 'zod';
import { MessageType } from '@/models/message';

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

export const SendMessageSchema = z.object({
  conversationId: z.string().min(1),
  senderId: z.string().min(1),
  type: z.nativeEnum(MessageType).default(MessageType.TEXT),
  content: z.string().min(1),
  attachments: z.array(MessageAttachmentSchema).optional(),
});

export type SendMessageRequest = z.infer<typeof SendMessageSchema>;

export const GetMessagesQuerySchema = z.object({
  conversationId: z.string().min(1),
  userId: z.string().min(1),
  cursor: z.string().optional(),
  limit: z.coerce.number().int().min(1).max(100).default(20),
});

export type GetMessagesQuery = z.infer<typeof GetMessagesQuerySchema>;
