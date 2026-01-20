import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import notificationRepository from '@src/repositories/notification'
import {
  NotificationSendRequest,
  NotificationSendResponse,
} from '@src/contracts/requests/notification.request'

export class NotificationController {
  /**
   * Send a new notification
   * POST /notification
   */
  @AsyncHandler
  public async sendNotification(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string

    const { channel, type, title, body, data } = req.body

    const newData = {
      userId,
      channel,
      type,
      title,
      body,
      data,
    } as NotificationSendRequest

    const notification = await notificationRepository.createAsync(newData)

    const dataResponse: NotificationSendResponse = {
      success: true,
      data: {
        userId: notification.userId,
        channel: notification.channel,
        status: notification.status,
        sentAt: notification.sentAt,
      },
    }

    return res.status(HTTP_STATUS_CODES.Created).json(dataResponse)
  }
}

export default new NotificationController()
