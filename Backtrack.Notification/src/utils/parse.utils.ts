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
function parseNotificationChannel(value: unknown) {
  const parsed = parseString(value)
  if (typeof parsed !== 'string') return undefined

  const channels = Object.values(NotificationChannel) as string[]
  const result = channels.includes(parsed)
    ? (parsed as NotificationChannelType)
    : undefined
  return result
}

/**
 * Safely parse a string to NotificationStatusType
 * @param value - The value to parse
 * @returns NotificationStatusType if valid, undefined otherwise
 */
function parseNotificationStatus(value: unknown) {
  const parsed = parseString(value)
  if (typeof parsed !== 'string') return undefined

  const statuses = Object.values(NotificationStatus) as string[]
  const result = statuses.includes(parsed)
    ? (parsed as NotificationStatusType)
    : undefined
  return result
}

/**
 * Safely parse a string to boolean
 * @param value - The value to parse
 * @returns boolean if valid, undefined otherwise
 */
function parseBoolean(value: unknown) {
  if (value === 'true') return true
  if (value === 'false') return false
  return undefined
}

/**
 * Safely parse to number
 * @param value - The value to parse
 * @returns number if valid, undefined otherwise
 */
function parseNumber(value: unknown) {
  const parsed = Number(value)
  return Number.isNaN(parsed) ? undefined : parsed
}

/**
 * Safely parse a string
 * @param value  The value to parse
 * @returns string if valid, undefined otherwise
 */
function parseString(value: unknown) {
  if (typeof value === 'string') return value
  return undefined
}

export const ParseUtils = {
  parseNotificationChannel,
  parseNotificationStatus,
  parseBoolean,
  parseNumber,
  parseString,
}
