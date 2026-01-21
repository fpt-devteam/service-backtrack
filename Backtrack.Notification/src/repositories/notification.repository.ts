import {
  GetNotificationsRequest,
  MarkAllAsReadRequest,
  MarkMultipleAsReadRequest,
  SendRequest,
} from '@src/contracts/requests/notification.request'
import { NotificationItem } from '@src/contracts/responses/notification.response'
import { Notification } from '@src/models/notification.model'
import { Types } from 'mongoose'

type SortOrder = 'asc' | 'desc'

class NotificationRepository {
  public async createAsync(notificationData: SendRequest) {
    const notification = new Notification(notificationData)
    return await notification.save()
  }

  public async markAllAsReadAsync(req: MarkAllAsReadRequest) {
    const { userId } = req

    const res = await Notification.updateMany(
      { userId, isRead: false },
      { $set: { isRead: true } },
    ).exec()

    return {
      matchedCount: res.matchedCount ?? 0,
      modifiedCount: res.modifiedCount ?? 0,
    }
  }

  public async markMultipleAsReadAsync(req: MarkMultipleAsReadRequest) {
    const { userId, notificationIds } = req

    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const res = await Notification.updateMany(
      { userId, _id: { $in: objectIds }, isRead: false },
      { $set: { isRead: true } },
    ).exec()

    return {
      matchedCount: res.matchedCount ?? 0,
      modifiedCount: res.modifiedCount ?? 0,
    }
  }

  public async findPaginatedAsync(request: GetNotificationsRequest) {
    const { limit = 5, sortField = 'createdAt', sortOrder = 'desc' } = request

    const filter = this.buildFilter(request)
    const sort = this.buildSort(sortField, sortOrder)

    const docs = await Notification.find(filter)
      .sort(sort)
      .limit(limit + 1)
      .lean()
      .exec()

    const { hasMore, items } = this.computePaging(docs, limit)
    const mappedItems = this.mapToNotificationItems(items)

    let nextCursor: string | null = null
    if (hasMore && items.length > 0) {
      nextCursor = this.getCursorValue(items[items.length - 1], sortField)
    }

    return {
      items: mappedItems,
      hasMore,
      nextCursor,
    }
  }

  // Validations
  public async checkExistingNotifications(
    notificationIds: string[],
  ): Promise<boolean> {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const count = await Notification.countDocuments({
      _id: { $in: objectIds },
    }).exec()

    return count === notificationIds.length
  }

  public async checkUserOwnsNotifications(
    userId: string,
    notificationIds: string[],
  ): Promise<boolean> {
    const objectIds = notificationIds.map((id) => new Types.ObjectId(id))

    const count = await Notification.countDocuments({
      _id: { $in: objectIds },
      userId,
    }).exec()

    return count === notificationIds.length
  }

  public checkAllNotificationValid(notificationIds: string[]): boolean {
    for (const id of notificationIds) {
      if (!Types.ObjectId.isValid(id)) {
        return false
      }
    }
    return true
  }

  // Helpers
  private buildFilter(request: GetNotificationsRequest) {
    const {
      userId,
      channel,
      status,
      isRead,
      cursor,
      sortField = 'createdAt',
      sortOrder = 'desc',
    } = request

    const filter: Record<string, any> = { userId }

    if (channel) filter.channel = channel
    if (status) filter.status = status
    if (isRead !== undefined) filter.isRead = isRead

    if (cursor) {
      const cursorDate = new Date(cursor)
      const op = sortOrder === 'asc' ? '$gt' : '$lt'
      filter[sortField] = { [op]: cursorDate }
    }

    return filter
  }

  private buildSort(sortField: string, sortOrder: SortOrder) {
    let direction: 1 | -1 = 1
    if (sortOrder === 'desc') direction = -1

    return { [sortField]: direction }
  }

  private computePaging<T>(docs: T[], limit: number) {
    let hasMore = false
    if (docs.length > limit) hasMore = true

    let items = docs
    if (hasMore) items = docs.slice(0, limit)

    return { hasMore, items }
  }

  private getCursorValue(lastItem: any, sortField: string): string | null {
    if (!lastItem) return null

    const value = lastItem[sortField]
    if (value === undefined || value === null) return null

    if (value instanceof Date) return value.toISOString()
    return value.toString()
  }

  private mapToNotificationItems(items: any[]): NotificationItem[] {
    return items.map((item) => ({
      _id: item._id.toString(),
      userId: item.userId,
      channel: item.channel,
      type: item.type,
      title: item.title ?? null,
      body: item.body ?? null,
      data: item.data,
      status: item.status,
      sentAt: item.sentAt,
      isRead: item.isRead,
      createdAt: item.createdAt,
      updatedAt: item.updatedAt,
    }))
  }
}

export default new NotificationRepository()
