import {
  NotificationSendPushRequest,
  NotificationOptions,
  NotificationStatusUpdateRequest,
} from '@src/contracts/requests/notification.request'
import { UserNotification } from '@src/contracts/responses/notification.response'
import { Device } from '@src/models/device.model'
import { Notification } from '@src/models/notification.model'
import expoPushProvider from '@src/providers/expo-push.provider'
import { NOTIFICATION_STATUS } from '@src/types/notification.type'
import { Model, Types } from 'mongoose'

const DEFAULT_LIMIT = 5

class NotificationRepository {
  constructor(private readonly model: Model<any>) {}

  public async filterAsync(userId: string, options: NotificationOptions) {
    const { cursor, status } = options

    const filter: any = { userId, status }
    if (cursor) filter.createdAt = { $lt: new Date(cursor) }

    const result = await Notification.find(filter)
      .sort({ createdAt: -1 })
      .limit(DEFAULT_LIMIT + 1)
      .lean()
      .exec()

    const hasMore = result.length > DEFAULT_LIMIT
    const notifications = hasMore ? result.slice(0, DEFAULT_LIMIT) : result

    const items: UserNotification[] = notifications.map((item: any) => ({
      id: item._id.toString(),
      userId: item.userId,
      type: item.type,
      title: item.title,
      body: item.body,
      data: item.data,
      status: item.status,
      sentAt: item.sentAt,
    }))

    const nextCursor =
      hasMore && items.length > 0
        ? items[items.length - 1].sentAt.toISOString()
        : null

    return {
      items,
      hasMore,
      nextCursor,
    }
  }

  public async updateStatusNotificationsAsync(
    userId: string,
    data: NotificationStatusUpdateRequest,
  ) {
    const { notificationIds, status } = data
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await this.model
      .updateMany(
        { userId, _id: { $in: objectIds }, status: { $ne: status } },
        { $set: { status } },
      )
      .exec()

    const result = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }

    return result
  }

  public async createAsync(payload: NotificationSendPushRequest) {
    const { target, source, title, body, data, type } = payload
    const { eventId, name } = source

    // Step 1: Check for existing notification (deduplication)
    const existNotification = await Notification.findOne({
      userId: target.userId,
      source: { name, eventId },
    })

    if (existNotification) {
      return {
        notificationId: existNotification._id.toString(),
        status: existNotification.status,
        deduped: true,
      }
    }

    // Step 2: Create new notification in DB with status Pending
    const notificationData = {
      userId: target.userId,
      type,
      title,
      body,
      data: data || null,
      status: NOTIFICATION_STATUS.Unread,
      source: { name, eventId },
      sentAt: new Date(),
    }

    const notification = await Notification.create(notificationData)

    // Step 3: Send push notification
    await this.sendPushNotification({
      userId: target.userId,
      title,
      body,
      data,
    })

    return {
      notificationId: notification._id.toString(),
      status: notification.status,
      deduped: false,
    }
  }

  // Validations
  public async checkExistNotifications(notificationIds: string[]) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const count = await this.model
      .countDocuments({
        _id: { $in: objectIds },
      })
      .exec()

    return count === notificationIds.length
  }

  public async checkAuthNotifications(
    userId: string,
    notificationIds: string[],
  ) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const count = await this.model
      .countDocuments({
        _id: { $in: objectIds },
        userId,
      })
      .exec()

    return count === notificationIds.length
  }

  public checkNotificationsValid(notificationIds: string[]) {
    for (const id of notificationIds) {
      if (!Types.ObjectId.isValid(id)) {
        return false
      }
    }
    return true
  }

  // Helpers
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
    } catch (error) {
      console.log('Error at send push notification:', error)
    }
  }
}

export default new NotificationRepository(Notification)
