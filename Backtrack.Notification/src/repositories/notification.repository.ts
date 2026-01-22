import { NotificationSendRequest } from '@src/contracts/requests/notification.request'
import { NotificationStatusUpdateResult } from '@src/contracts/responses/notification.response'
import { Notification } from '@src/models/notification.model'
import { Model, Types } from 'mongoose'

class NotificationRepository {
  constructor(private readonly model: Model<any>) {}

  public async createAsync(data: NotificationSendRequest) {
    const notification = new Notification(data)
    return await notification.save()
  }

  public async updateAllReadStatusAsync(userId: string, newStatus: boolean) {
    const res = await this.model
      .updateMany(
        { userId, isRead: !newStatus },
        { $set: { isRead: newStatus } },
      )
      .exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }
    return result
  }

  public async updateMutipleReadStatusAsync(
    userId: string,
    notificationIds: string[],
    newStatus: boolean,
  ) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await this.model
      .updateMany(
        { userId, _id: { $in: objectIds }, isRead: !newStatus },
        { $set: { isRead: newStatus } },
      )
      .exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }

    return result
  }

  public async updateAllArchivedStatusAsync(
    userId: string,
    newStatus: boolean,
  ) {
    const res = await this.model
      .updateMany(
        { userId, isArchived: !newStatus },
        { $set: { isArchived: newStatus } },
      )
      .exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }
    return result
  }

  public async updateMutipleArchivedStatusAsync(
    userId: string,
    notificationIds: string[],
    newStatus: boolean,
  ) {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await this.model
      .updateMany(
        { userId, _id: { $in: objectIds }, isArchived: !newStatus },
        { $set: { isArchived: newStatus } },
      )
      .exec()

    const result: NotificationStatusUpdateResult = {
      matchedCount: res.matchedCount,
      modifiedCount: res.modifiedCount,
    }

    return result
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
}

export default new NotificationRepository(Notification)
