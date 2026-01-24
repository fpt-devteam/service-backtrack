import { NotificationSendPushRequest, NotificationOptions } from '@src/contracts/requests/notification.request'
import { NotificationItem, NotificationStatusUpdateResult } from '@src/contracts/responses/notification.response'
import { Device } from '@src/models/device.model'
import { Notification } from '@src/models/notification.model'
import expoPushProvider from '@src/providers/expo-push.provider'
import { NotificationStatus } from '@src/types/notification.type'
import { Model, Types } from 'mongoose'

class NotificationRepository {
  constructor(private readonly model: Model<any>) {}

  public async filterAsync(userId: string, options: NotificationOptions) {
    const { cursor, limit, channel, status, isRead, isArchived } = options
    const filter: any = { userId }

    if (cursor) filter.createdAt = { $lt: new Date(cursor) }

    if (channel !== undefined) filter.channel = channel

    if (status !== undefined) {
      filter.status = status
    }
    if (isRead !== undefined) {
      filter.isRead = isRead
    }
    if (isArchived !== undefined) {
      filter.isArchived = isArchived
    }

    const items = await Notification.find(filter)
      .sort({ createdAt: -1 })
      .limit(limit + 1)
      .lean()
      .exec()

    const hasMore = items.length > limit
    const notifications = hasMore ? items.slice(0, limit) : items

    const transformedNotifications: NotificationItem[] = notifications.map((item: any) => ({
      _id: item._id.toString(),
      userId: item.userId,
      channel: item.channel,
      type: item.type,
      title: item.title,
      body: item.body,
      data: item.data,
      status: item.status,
      sentAt: item.sentAt,
      isRead: item.isRead,
      isArchived: item.isArchived,
      createdAt: item.createdAt,
      updatedAt: item.updatedAt,
    }))

    const nextCursor = hasMore && transformedNotifications.length > 0 ? transformedNotifications[transformedNotifications.length - 1].createdAt.toISOString() : null

    return {
      items: transformedNotifications,
      hasMore,
      nextCursor,
    }
  }

  public async updateAllReadStatusAsync(userId: string, newStatus: boolean) {
    const res = await this.model.updateMany({ userId, isRead: !newStatus }, { $set: { isRead: newStatus } }).exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }
    return result
  }

  public async updateMutipleReadStatusAsync(userId: string, notificationIds: string[], newStatus: boolean) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await this.model.updateMany({ userId, _id: { $in: objectIds }, isRead: !newStatus }, { $set: { isRead: newStatus } }).exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }

    return result
  }

  public async updateAllArchivedStatusAsync(userId: string, newStatus: boolean) {
    const res = await this.model.updateMany({ userId, isArchived: !newStatus }, { $set: { isArchived: newStatus } }).exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }
    return result
  }

  public async updateMutipleArchivedStatusAsync(userId: string, notificationIds: string[], newStatus: boolean) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await this.model.updateMany({ userId, _id: { $in: objectIds }, isArchived: !newStatus }, { $set: { isArchived: newStatus } }).exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }

    return result
  }

  public async createAsync(payload: NotificationSendPushRequest) {
    const { target, source, provider, title, body, data, type, channel } = payload
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
      channel,
      status: NotificationStatus.Pending,
      source: { name, eventId },
      provider: provider,
      isRead: false,
      isArchived: false,
      sentAt: new Date(),
    }
    const notification = await Notification.create(notificationData)

    // Step 3: Send push notification
    await this.sendPushNotification(notification._id.toString(), { userId: target.userId, title, body, data })

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

  public async checkAuthNotifications(userId: string, notificationIds: string[]) {
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

  private async sendPushNotification(
    notificationId: string,
    message: {
      userId: string
      title?: string | null
      body?: string | null
      data?: Record<string, unknown>
    },
  ) {
    try {
      const { userId, title, body, data } = message
      const allDevices = await Device.find({ userId, isActive: true })
      const tokens = allDevices.map((device) => device.token)
      const results = await expoPushProvider.sendToTokens(tokens, { title, body, data })

      const successCount = results.filter((r) => r.success).length
      const status = successCount > 0 ? NotificationStatus.Sent : NotificationStatus.Failed
      await Notification.findByIdAndUpdate(notificationId, { status, sentAt: new Date() })
    } catch (error) {
      console.log('Error at send push notification:', error)
      const status = NotificationStatus.Failed
      await Notification.findByIdAndUpdate(notificationId, { status, sentAt: new Date() })
    }
  }
}

export default new NotificationRepository(Notification)
