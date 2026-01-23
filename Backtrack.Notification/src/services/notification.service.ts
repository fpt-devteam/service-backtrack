import notificationRepository from '@src/repositories/notification.repository'
import { NotificationSendPushRequest, ArchivedStatusUpdateRequest, ReadStatusUpdateRequest, ReadStatusUpdateAllRequest, ArchivedStatusUpdateAllRequest, NotificationOptions } from '@src/contracts/requests/notification.request'
import { ErrorCodes } from '@src/common/errors/error.constants'

class NotificationService {
  public async filterAsync(userId: string, options: NotificationOptions) {
    const result = await notificationRepository.filterAsync(userId, options)
    return result
  }

  public async createAsync(payload: NotificationSendPushRequest) {
    const result = await notificationRepository.createAsync(payload)
    return result
  }

  public async updateArchivedStatusAsync(userId: string, req: ArchivedStatusUpdateRequest) {
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
    const allExist = await notificationRepository.checkExistNotifications(notificationIds)
    if (!allExist) {
      throw ErrorCodes.NotificationsNotFound
    }

    // Validation: Check if all notifications belong to the user
    const allBelongToUser = await notificationRepository.checkAuthNotifications(userId, notificationIds)
    if (!allBelongToUser) {
      throw ErrorCodes.NotificationOwnershipError
    }

    const result = await notificationRepository.updateMutipleArchivedStatusAsync(userId, notificationIds, isArchived)
    return result
  }

  public async updateAllArchivedStatusAsync(userId: string, req: ArchivedStatusUpdateAllRequest) {
    const { isArchived } = req
    const result = await notificationRepository.updateAllArchivedStatusAsync(userId, isArchived)
    return result
  }

  public async updateReadStatusAsync(userId: string, req: ReadStatusUpdateRequest) {
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
    const allExist = await notificationRepository.checkExistNotifications(notificationIds)
    if (!allExist) {
      throw ErrorCodes.NotificationsNotFound
    }

    // Validation: Check if all notifications belong to the user
    const allBelongToUser = await notificationRepository.checkAuthNotifications(userId, notificationIds)
    if (!allBelongToUser) {
      throw ErrorCodes.NotificationOwnershipError
    }

    const result = await notificationRepository.updateMutipleReadStatusAsync(userId, notificationIds, isRead)
    return result
  }

  public async updateAllReadStatusAsync(userId: string, req: ReadStatusUpdateAllRequest) {
    const { isRead } = req
    const result = await notificationRepository.updateAllReadStatusAsync(userId, isRead)
    return result
  }
}

export default new NotificationService()
