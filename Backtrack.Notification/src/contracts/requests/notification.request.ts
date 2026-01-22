import { Nullable, Optional, PaginationOptions } from '@src/types/shared.type'
import { z } from 'zod'

import {
  NotificationChannelType,
  NotificationStatusType,
  NotificationType,
} from '@src/types/notification.type'

export type NotificationsQueryRequest = {
  userId: string
  options: NotificationsOptions
}

export type NotificationsOptions = PaginationOptions & {
  channel?: NotificationChannelType
  status?: NotificationStatusType
  isRead?: boolean
  isArchived?: boolean
}

export type NotificationSendRequest = {
  channel: NotificationChannelType
  type: NotificationType
  title: Optional<Nullable<string>>
  body: Optional<Nullable<string>>
  data: Optional<Record<string, unknown>>
}

//
export const NotificationIdsSchema = z
  .array(z.string().min(1, 'NotificationId must be a non-empty string'))
  .min(1, 'NotificationIds must contain at least 1 id')

//
export const ReadStatusUpdateAllRequestSchema = z
  .object({ isRead: z.boolean() })
  .strict()

export type ReadStatusUpdateRequest = z.infer<
  typeof ReadStatusUpdateRequestSchema
>

//
export const ReadStatusUpdateRequestSchema = z.object({
  notificationIds: NotificationIdsSchema,
  isRead: z.boolean(),
})

export type ReadStatusUpdateAllRequest = z.infer<
  typeof ReadStatusUpdateAllRequestSchema
>

//
export const ArchivedStatusUpdateRequestSchema = z.object({
  notificationIds: NotificationIdsSchema,
  isArchived: z.boolean(),
})

export type ArchivedStatusUpdateRequest = z.infer<
  typeof ArchivedStatusUpdateRequestSchema
>

//
export const ArchivedStatusUpdateAllRequestSchema = z
  .object({ isArchived: z.boolean() })
  .strict()

export type ArchivedStatusUpdateAllRequest = z.infer<
  typeof ArchivedStatusUpdateAllRequestSchema
>
