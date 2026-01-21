import notificationRepository from '@src/repositories/notification.repository'
import {
  GetNotificationsRequest,
  MarkAllAsReadRequest,
  MarkMultipleAsReadRequest,
  SendRequest,
} from '@src/contracts/requests/notification.request'
import { ErrorCodes } from '@src/common/errors/error.constants'

class NotificationService {
  public async sendNotification(userId: string, requestData: SendRequest) {
    const notificationData = {
      ...requestData,
      userId,
    }

    // TODO: Send to external service (e.g., email, SMS, push notification)
    // await externalNotificationService.send(notificationData)

    // Save to database
    const notification =
      await notificationRepository.createAsync(notificationData)

    return {
      userId: notification.userId,
      channel: notification.channel,
      status: notification.status,
      sentAt: notification.sentAt,
    }
  }

  public async markAllAsRead(userId: string) {
    const request: MarkAllAsReadRequest = { userId }
    const result = await notificationRepository.markAllAsReadAsync(request)
    return result
  }

  public async markMultipleAsRead(userId: string, notificationIds: string[]) {
    // Validate input
    if (!Array.isArray(notificationIds) || notificationIds.length === 0) {
      throw ErrorCodes.MissingNotificationIds
    }

    // Validate all IDs are valid ObjectIds
    const allValid =
      notificationRepository.checkAllNotificationValid(notificationIds)
    if (!allValid) {
      throw ErrorCodes.InvalidNotificationIds
    }

    // Check all notifications exist
    const allExist =
      await notificationRepository.checkExistingNotifications(notificationIds)
    if (!allExist) {
      throw ErrorCodes.NotificationsNotFound
    }

    // Check user owns all notifications
    const userOwnsAll = await notificationRepository.checkUserOwnsNotifications(
      userId,
      notificationIds,
    )
    if (!userOwnsAll) {
      throw ErrorCodes.NotificationOwnershipError
    }

    // Mark multiple as read
    const request: MarkMultipleAsReadRequest = {
      userId,
      notificationIds,
    }

    const result = await notificationRepository.markMultipleAsReadAsync(request)
    return result
  }

  public async getNotifications(request: GetNotificationsRequest) {
    const result = await notificationRepository.findPaginatedAsync(request)
    return result
  }
}

export default new NotificationService()
