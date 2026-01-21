import { ApiResponse, PaginatedResponse } from '@src/types/shared.type'
import {
  NotificationChannelType,
  NotificationStatusType,
  NotificationType,
} from '@src/types/notification.type'

export type NotificationItem = {
  _id: string
  userId: string
  channel: NotificationChannelType
  type: NotificationType
  title: string | null
  body: string | null
  data: Record<string, unknown> | null
  status: NotificationStatusType
  sentAt: Date
  isRead: boolean
  createdAt: Date
  updatedAt: Date
}

export type GetNotificationsResult = PaginatedResponse<NotificationItem[]>

export type SendResult = {
  userId: string
  channel: NotificationChannelType
  status: NotificationStatusType
  sentAt: Date
}

export type MarkAllAsReadResult = {
  matchedCount: number
  modifiedCount: number
}

export type MarkMultipleAsReadResult = {
  matchedCount: number
  modifiedCount: number
}

export type SendResponse = ApiResponse<SendResult>

export type GetNotificationsResponse = ApiResponse<GetNotificationsResult>

export type MarkAllAsReadResponse = ApiResponse<MarkAllAsReadResult>

export type MarkMultipleAsReadResponse = ApiResponse<MarkMultipleAsReadResult>
