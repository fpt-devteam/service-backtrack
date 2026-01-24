import { z } from 'zod'
import {
  NOTIFICATION_EVENT,
  NOTIFICATION_STATUS,
} from '@src/types/notification.type'

export const NotificationIdsSchema = z
  .array(z.string().min(1, 'NotificationId must be a non-empty string'))
  .min(1, 'NotificationIds must contain at least 1 id')

export const NotificationStatusUpdateRequestSchema = z.object({
  notificationIds: NotificationIdsSchema,
  status: z.enum(Object.values(NOTIFICATION_STATUS)),
})

export type NotificationStatusUpdateRequest = z.infer<
  typeof NotificationStatusUpdateRequestSchema
>

//
export const NotificationSendPushRequestSchema = z.object({
  target: z.object({ userId: z.string().min(1, 'UserId is required').trim() }),
  source: z.object({
    name: z.string().min(1, 'Source name is required').trim(),
    eventId: z.string().min(1, 'SourceEventId is required').trim(),
  }),
  title: z.string().trim().min(1, 'Title is required'),
  body: z.string().trim().min(1, 'Body is required'),
  data: z.record(z.string(), z.unknown()).optional(),
  type: z.enum(Object.values(NOTIFICATION_EVENT)),
})

export type NotificationSendPushRequest = z.infer<
  typeof NotificationSendPushRequestSchema
>

export const NotificationOptionsSchema = z.object({
  cursor: z.string().optional(),
  status: z.enum(Object.values(NOTIFICATION_STATUS)),
})

export type NotificationOptions = z.infer<typeof NotificationOptionsSchema>
