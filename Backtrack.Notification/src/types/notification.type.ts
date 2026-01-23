/* */
export const NotificationChannel = {
  Push: 'Push',
  InApp: 'InApp',
  Email: 'Email',
} as const
export type NotificationChannelType = (typeof NotificationChannel)[keyof typeof NotificationChannel]

/* */
export const NotificationEvent = {
  ChatEvent: 'ChatEvent',
} as const
export type NotificationType = (typeof NotificationEvent)[keyof typeof NotificationEvent]

/* */
export const NotificationStatus = {
  Pending: 'Pending',
  Sent: 'Sent',
  Failed: 'Failed',
} as const
export type NotificationStatusType = (typeof NotificationStatus)[keyof typeof NotificationStatus]

/* */
export const PushProviders = {
  Expo: 'Expo',
  FCM: 'FCM',
  APNs: 'APNs',
} as const

export type PushProviderType = (typeof PushProviders)[keyof typeof PushProviders]
