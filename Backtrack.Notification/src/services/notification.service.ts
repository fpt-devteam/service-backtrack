import notificationRepository from '@src/repositories/notification.repository'
import {
  NotificationSendRequest,
  ArchivedStatusUpdateRequest,
  ReadStatusUpdateRequest,
  ReadStatusUpdateAllRequest,
  ArchivedStatusUpdateAllRequest,
} from '@src/contracts/requests/notification.request'
import { ErrorCodes } from '@src/common/errors/error.constants'

class NotificationService {
  public async sendNotification(
    userId: string,
    requestData: NotificationSendRequest,
  ) {
    const notificationData = {
      ...requestData,
      userId,
    }

    const message = {
      to: 'ExponentPushToken[L7uO_mNAP4deMkwYs3kyxL]',
      sound: 'default',
      title: requestData.title,
      body: requestData.body,
      data: requestData.data || {},
    }

    const response = await fetch('https://exp.host/--/api/v2/push/send', {
      method: 'POST',
      headers: {
        Accept: 'application/json',
        'Accept-encoding': 'gzip, deflate',
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(message),
    })

    console.log('Send: ', response)

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

  public async updateArchivedStatusAsync(
    userId: string,
    req: ArchivedStatusUpdateRequest,
  ) {
    const { notificationIds, isArchived } = req

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

    const result =
      await notificationRepository.updateMutipleArchivedStatusAsync(
        userId,
        notificationIds,
        isArchived,
      )
    return result
  }

  public async updateAllArchivedStatusAsync(
    userId: string,
    req: ArchivedStatusUpdateAllRequest,
  ) {
    const { isArchived } = req
    const result = await notificationRepository.updateAllArchivedStatusAsync(
      userId,
      isArchived,
    )
    return result
  }

  public async updateReadStatusAsync(
    userId: string,
    req: ReadStatusUpdateRequest,
  ) {
    const { isRead, notificationIds } = req

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

    const result = await notificationRepository.updateMutipleReadStatusAsync(
      userId,
      notificationIds,
      isRead,
    )
    return result
  }

  public async updateAllReadStatusAsync(
    userId: string,
    req: ReadStatusUpdateAllRequest,
  ) {
    const { isRead } = req
    const result = await notificationRepository.updateAllReadStatusAsync(
      userId,
      isRead,
    )
    return result
  }
}

export default new NotificationService()
