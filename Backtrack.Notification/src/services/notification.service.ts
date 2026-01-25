import notificationRepository from '@src/repositories/notification.repository'
import {
  NotificationSendPushRequest,
  NotificationOptions,
  NotificationStatusUpdateRequest,
} from '@src/contracts/requests/notification.request'
import { ErrorCodes } from '@src/common/errors'

class NotificationService {
  public async filterAsync(userId: string, options: NotificationOptions) {
    const result = await notificationRepository.filterAsync(userId, options)
    return result
  }

  public async getUnreadCountAsync(userId: string) {
    const result = await notificationRepository.getUnreadCountAsync(userId)
    return result
  }

  public async createAsync(payload: NotificationSendPushRequest) {
    const result = await notificationRepository.createAsync(payload)
    return result
  }

  public async updateStatusNotificationsAsync(
    userId: string,
    req: NotificationStatusUpdateRequest,
  ) {
    const { notificationIds } = req
    // Validation: Check if notificationIds is provided
    if (!notificationIds || notificationIds.length === 0) {
      throw ErrorCodes.MissingNotificationIds
    }

    // Validation: Check if all notificationIds are valid MongoDB ObjectIds
    if (!notificationRepository.checkNotificationsValid(notificationIds)) {
      throw ErrorCodes.InvalidNotificationIds
    }

    // Validation: Check if all notifications exist
    const allExist =
      await notificationRepository.checkExistNotifications(notificationIds)
    if (!allExist) {
      throw ErrorCodes.NotificationsNotFound
    }

    // Validation: Check if all notifications belong to the user
    const allBelongToUser = await notificationRepository.checkAuthNotifications(
      userId,
      notificationIds,
    )
    if (!allBelongToUser) {
      throw ErrorCodes.NotificationOwnershipError
    }

    const result = await notificationRepository.updateStatusNotificationsAsync(
      userId,
      req,
    )
    return result
  }
}

export default new NotificationService()
