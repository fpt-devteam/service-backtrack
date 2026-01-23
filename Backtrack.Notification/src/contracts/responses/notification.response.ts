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
  isArchived: boolean
  createdAt: Date
  updatedAt: Date
}

export type NotificationsGetResult = PaginatedResponse<NotificationItem[]>

export type NotificationSendResult = {
  userId: string
  channel: NotificationChannelType
  status: NotificationStatusType
  sentAt: Date
}

export type NotificationStatusUpdateResult = {
  matchedCount: number
  modifiedCount: number
}

export type NotificationSendResponse = ApiResponse<NotificationSendResult>

export type NotificationGetResponse = ApiResponse<NotificationsGetResult>

export type NotificationStatusUpdateResponse =
  ApiResponse<NotificationStatusUpdateResult>
