import { ApiResponse, Nullable, Optional } from '@src/types/global.type'
import {
  NotificationChannelType,
  NotificationStatusType,
  NotificationType,
} from '@src/types/notification.type'

export type NotificationSendRequest = {
  userId: string
  channel: NotificationChannelType
  type: NotificationType
  title: Optional<Nullable<string>>
  body: Optional<Nullable<string>>
  data: Optional<Record<string, unknown>>
}

export type NotificationSendResponse = ApiResponse<{
  userId: string
  channel: NotificationChannelType
  status: NotificationStatusType
  sentAt: Date
}>
