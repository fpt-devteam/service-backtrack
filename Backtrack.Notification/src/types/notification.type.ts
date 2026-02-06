/*
 * ENUMS & TYPES
 */
export const NOTIFICATION_STATUS = {
  Unread: 'Unread',
  Read: 'Read',
  Archived: 'Archived',
} as const

export type NotificationStatus = (typeof NOTIFICATION_STATUS)[keyof typeof NOTIFICATION_STATUS]

export const NOTIFICATION_EVENT = {
  ChatEvent: "ChatEvent",
  AIMatchingEvent: "AIMatchingEvent",
  QRScanEvent: "QRScanEvent",
  SystemAlertEvent: "SystemAlertEvent",
} as const

export type NotificationEvent = (typeof NOTIFICATION_EVENT)[keyof typeof NOTIFICATION_EVENT]
