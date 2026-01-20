import { ApiResponse } from '@src/types/global.type'
import {
  NotificationChannelType,
  NotificationStatusType,
} from '@src/types/notification.type'

export type NotificationSendResponse = ApiResponse<{
  userId: string
  channel: NotificationChannelType
  status: NotificationStatusType
  sentAt: Date
}>
