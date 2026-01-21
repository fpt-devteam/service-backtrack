import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import notificationService from '@src/services/notification.service'
import { SendRequest } from '@src/contracts/requests/notification.request'
import {
  GetNotificationsResponse,
  MarkAllAsReadResponse,
  MarkMultipleAsReadResponse,
  SendResponse,
} from '@src/contracts/responses/notification.response'
import {
  parseBoolean,
  parseNotificationChannel,
  parseNotificationStatus,
  parseNumber,
} from '@src/utils/type-parsers'

export class NotificationController {
  @AsyncHandler
  public async sendNotification(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const requestData = req.body as SendRequest

    const notification = await notificationService.sendNotification(
      userId,
      requestData,
    )

    const response: SendResponse = {
      success: true,
      data: notification,
    }

    return res.status(HTTP_STATUS_CODES.Created).json(response)
  }

  @AsyncHandler
  public async getNotifications(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const { cursor, limit, channel, status, isRead } = req.query

    const result = await notificationService.getNotifications({
      userId,
      cursor: cursor as string | undefined,
      limit: parseNumber(limit),
      channel: parseNotificationChannel(channel),
      status: parseNotificationStatus(status),
      isRead: parseBoolean(isRead),
    })

    const response: GetNotificationsResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async markAllAsRead(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string

    const result = await notificationService.markAllAsRead(userId)

    const response: MarkAllAsReadResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async markMultipleAsRead(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const notificationIds = req.body.notificationIds as string[]

    const result = await notificationService.markMultipleAsRead(
      userId,
      notificationIds,
    )

    const response: MarkMultipleAsReadResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }
}

export default new NotificationController()
