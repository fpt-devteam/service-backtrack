import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import notificationService from '@src/services/notification.service'
import {
  NotificationSendPushRequestSchema,
  NotificationOptionsSchema,
  NotificationStatusUpdateRequestSchema,
} from '@src/contracts/requests/notification.request'
import {
  UserNotificationFilterResponse,
  UnreadCountResponse,
} from '@src/contracts/responses/notification.response'

export class NotificationController {
  @AsyncHandler
  public async createAsync(req: Request, res: Response) {
    const payload = NotificationSendPushRequestSchema.parse(req.body)
    const result = await notificationService.createAsync(payload)

    const response = {
      success: true,
      data: {
        notificationId: result.notificationId,
        status: result.status,
        deduped: result.deduped,
      },
    }

    return res.status(HTTP_STATUS_CODES.Created).json(response)
  }

  @AsyncHandler
  public async getAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const options = NotificationOptionsSchema.parse(req.query)
    const result = await notificationService.filterAsync(userId, options)

    const response: UserNotificationFilterResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async getUnreadCountAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const result = await notificationService.getUnreadCountAsync(userId)

    const response: UnreadCountResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async updateStatusNotificationsAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const request = NotificationStatusUpdateRequestSchema.parse(req.body)

    await notificationService.updateStatusNotificationsAsync(userId, request)
    return res.status(HTTP_STATUS_CODES.Ok).json()
  }
}

export default new NotificationController()
