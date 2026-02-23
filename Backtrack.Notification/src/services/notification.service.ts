import notificationRepository from '@src/repositories/notification.repository'
import {
  NotificationSendPushRequest,
  NotificationOptions,
  NotificationStatusUpdateRequest,
} from '@src/contracts/requests/notification.request'
import { ErrorCodes } from '@src/common/errors'
import { Device } from '@src/models/device.model'
import expoPushProvider from '@src/providers/expo-push.provider'
import { NOTIFICATION_CATEGORY, NOTIFICATION_STATUS } from '@src/types/notification.type'
import { getSocketInstance } from '@src/socket'

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
    const { notification, deduped } = await notificationRepository.createAsync(payload)

    if (deduped) {
      return {
        notificationId: notification._id.toString(),
        status: notification.status,
        deduped: true,
      }
    }

    if (notification.category === NOTIFICATION_CATEGORY.Push) {
      await this.sendPushNotification({
        userId: payload.target.userId,
        title: payload.title,
        body: payload.body,
        data: payload.data,
      })
    }

    return {
      notificationId: notification._id.toString(),
      status: notification.status,
      deduped: false,
    }
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

  private async sendPushNotification(message: {
    userId: string
    title?: string | null
    body?: string | null
    data?: Record<string, unknown>
  }) {
    try {
      const { userId, title, body, data } = message
      const allDevices = await Device.find({ userId, isActive: true })

      const tokens = allDevices.map((device) => device.token)
      await expoPushProvider.sendToTokens(tokens, { title, body, data })
      await this.updateUnreadCount(userId)
    } catch (error) {
      console.log('Error at send push notification:', error)
    }
  }

  private async updateUnreadCount(userId: string) {
    try {
      const allDevices = await Device.find({ userId, isActive: true })
      const notificationCount = await notificationRepository.getUnreadCountAsync(userId)

      const io = getSocketInstance();
      io.to(allDevices.map((d) => d.deviceId)).emit('unreadCountUpdated', {
        userId,
        count: notificationCount,
      })
    } catch (error) {
      console.log('Error at send push notification:', error)
    }
  }
}

export default new NotificationService()
