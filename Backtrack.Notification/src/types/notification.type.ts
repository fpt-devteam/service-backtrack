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

export const NOTIFICATION_CATEGORY = {
  InApp: "InApp",
  Push: "Push",
} as const

export type NotificationEvent = (typeof NOTIFICATION_EVENT)[keyof typeof NOTIFICATION_EVENT]
export type NotificationCategory = (typeof NOTIFICATION_CATEGORY)[keyof typeof NOTIFICATION_CATEGORY]