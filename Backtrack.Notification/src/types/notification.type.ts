export const NotificationChannel = {
  Push: 'Push',
  InApp: 'InApp',
  Email: 'Email',
} as const

export const NotificationEvent = {
  ChatEvent: 'ChatEvent',
} as const

export const NotificationStatus = {
  Pending: 'Pending',
  Sent: 'Sent',
  Failed: 'Failed',
} as const

export type NotificationStatusType =
  (typeof NotificationStatus)[keyof typeof NotificationStatus]

export type NotificationType =
  (typeof NotificationEvent)[keyof typeof NotificationEvent]

export type NotificationChannelType =
  (typeof NotificationChannel)[keyof typeof NotificationChannel]
