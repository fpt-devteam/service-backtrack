import {
  NotificationChannel,
  NotificationChannelType,
  NotificationStatus,
  NotificationStatusType,
} from '@src/types/notification.type'

/**
 * Safely parse a string to NotificationChannelType
 * @param value - The value to parse
 * @returns NotificationChannelType if valid, undefined otherwise
 */
export function parseNotificationChannel(
  value: unknown,
): NotificationChannelType | undefined {
  if (typeof value !== 'string') {
    return undefined
  }

  const channels = Object.values(NotificationChannel) as string[]
  return channels.includes(value)
    ? (value as NotificationChannelType)
    : undefined
}

/**
 * Safely parse a string to NotificationStatusType
 * @param value - The value to parse
 * @returns NotificationStatusType if valid, undefined otherwise
 */
export function parseNotificationStatus(
  value: unknown,
): NotificationStatusType | undefined {
  if (typeof value !== 'string') {
    return undefined
  }

  const statuses = Object.values(NotificationStatus) as string[]
  return statuses.includes(value)
    ? (value as NotificationStatusType)
    : undefined
}

/**
 * Safely parse a string to boolean
 * @param value - The value to parse
 * @returns boolean if valid, undefined otherwise
 */
export function parseBoolean(value: unknown): boolean | undefined {
  if (value === 'true') return true
  if (value === 'false') return false
  return undefined
}

/**
 * Safely parse a string to number
 * @param value - The value to parse
 * @returns number if valid, undefined otherwise
 */
export function parseNumber(value: unknown) {
  const parsed = Number(value)
  return Number.isNaN(parsed) ? undefined : parsed
}
