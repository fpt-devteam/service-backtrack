import { NotificationSendRequest } from '@src/contracts/requests/notification.request'
import { Notification } from '@src/models/notification.model'

class NotificationRepository {
  public async createAsync(notificationData: NotificationSendRequest) {
    const notification = new Notification(notificationData)
    const saved = await notification.save()
    return saved
  }
}

export default new NotificationRepository()
