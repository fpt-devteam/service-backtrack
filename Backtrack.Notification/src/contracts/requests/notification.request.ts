import { Nullable, Optional, PaginationOptions } from '@src/types/shared.type'
import {
  NotificationChannelType,
  NotificationStatusType,
  NotificationType,
} from '@src/types/notification.type'

export type GetNotificationsRequest = PaginationOptions & {
  userId: string
  channel?: NotificationChannelType
  status?: NotificationStatusType
  isRead?: boolean
}

export type SendRequest = {
  channel: NotificationChannelType
  type: NotificationType
  title: Optional<Nullable<string>>
  body: Optional<Nullable<string>>
  data: Optional<Record<string, unknown>>
}

export type MarkAllAsReadRequest = {
  userId: string
}

export type MarkMultipleAsReadRequest = {
  userId: string
  notificationIds: string[]
}
