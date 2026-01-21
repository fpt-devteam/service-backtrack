import HTTP_STATUS_CODES from '../constants/HTTP_STATUS_CODES'
import { AppError } from './app-error'

export const ErrorCodes = {
  MissingNotificationId: new AppError(
    'MissingNotificationId',
    'Notification ID is required',
    HTTP_STATUS_CODES.BadRequest,
  ),
  MissingNotificationIds: new AppError(
    'MissingNotificationIds',
    'NotificationIds is required',
    HTTP_STATUS_CODES.BadRequest,
  ),
  InvalidNotificationIds: new AppError(
    'InvalidNotificationIds',
    'Some notification IDs are invalid',
    HTTP_STATUS_CODES.BadRequest,
  ),
  NotificationsNotFound: new AppError(
    'NotificationsNotFound',
    'Some notifications do not exist',
    HTTP_STATUS_CODES.BadRequest,
  ),
  NotificationOwnershipError: new AppError(
    'NotificationOwnershipError',
    'Some notifications do not belong to the user',
    HTTP_STATUS_CODES.Forbidden,
  ),
}
