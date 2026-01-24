import { ApiResponse, PaginatedResponse } from '@src/types/shared.type'
import {
  NotificationEvent,
  NotificationStatus,
} from '@src/types/notification.type'

export type UserNotification = {
  id: string
  userId: string
  type: NotificationEvent
  title: string
  body: string
  data?: Record<string, unknown>
  status: NotificationStatus
  sentAt: Date
}

export type UserNotificationFilterResult = PaginatedResponse<UserNotification[]>
export type UserNotificationFilterResponse =
  ApiResponse<UserNotificationFilterResult>
