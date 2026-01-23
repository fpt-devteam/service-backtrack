import { z } from 'zod'

import { NotificationChannel, NotificationEvent, NotificationStatus, PushProviders } from '@src/types/notification.type'

// Helper function to parse boolean query params
const parseBooleanString = (val: string | undefined): boolean | undefined => {
  if (val === 'true') return true
  if (val === 'false') return false
  return undefined
}

export const NotificationOptionsSchema = z.object({
  cursor: z.string().optional(),
  limit: z
    .string()
    .optional()
    .transform((val) => (val ? Number.parseInt(val, 10) : 20))
    .pipe(z.number().min(1).max(50)),
  channel: z.enum(Object.values(NotificationChannel)).optional(),
  status: z.enum(Object.values(NotificationStatus)).optional(),
  isRead: z.string().optional().transform(parseBooleanString).pipe(z.boolean().optional()),
  isArchived: z.string().optional().transform(parseBooleanString).pipe(z.boolean().optional()),
})
export type NotificationOptions = z.infer<typeof NotificationOptionsSchema>

//
export const NotificationIdsSchema = z.array(z.string().min(1, 'NotificationId must be a non-empty string')).min(1, 'NotificationIds must contain at least 1 id')

//
export const ReadStatusUpdateAllRequestSchema = z.object({ isRead: z.boolean() }).strict()
export type ReadStatusUpdateAllRequest = z.infer<typeof ReadStatusUpdateAllRequestSchema>

//
export const ReadStatusUpdateRequestSchema = z.object({ notificationIds: NotificationIdsSchema, isRead: z.boolean() })
export type ReadStatusUpdateRequest = z.infer<typeof ReadStatusUpdateRequestSchema>

//
export const ArchivedStatusUpdateRequestSchema = z.object({ notificationIds: NotificationIdsSchema, isArchived: z.boolean() })
export type ArchivedStatusUpdateRequest = z.infer<typeof ArchivedStatusUpdateRequestSchema>

//
export const ArchivedStatusUpdateAllRequestSchema = z.object({ isArchived: z.boolean() }).strict()
export type ArchivedStatusUpdateAllRequest = z.infer<typeof ArchivedStatusUpdateAllRequestSchema>

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
  type: z.enum(Object.values(NotificationEvent)),
  channel: z.enum(Object.values(NotificationChannel)),
  provider: z.enum(Object.values(PushProviders)),
})

export type NotificationSendPushRequest = z.infer<typeof NotificationSendPushRequestSchema>
