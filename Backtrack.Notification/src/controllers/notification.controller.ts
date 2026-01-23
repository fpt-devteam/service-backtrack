import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import { HEADERS } from '@src/utils/headers'
import notificationService from '@src/services/notification.service'
import { NotificationSendPushRequestSchema, ArchivedStatusUpdateAllRequestSchema, ArchivedStatusUpdateRequestSchema, NotificationOptionsSchema, ReadStatusUpdateAllRequestSchema, ReadStatusUpdateRequestSchema } from '@src/contracts/requests/notification.request'
import { NotificationGetResponse, NotificationStatusUpdateResponse } from '@src/contracts/responses/notification.response'

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

    const response: NotificationGetResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async updateReadStatusAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const request = ReadStatusUpdateRequestSchema.parse(req.body)

    const result = await notificationService.updateReadStatusAsync(userId, request)

    const response: NotificationStatusUpdateResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async updateArchivedStatusAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const request = ArchivedStatusUpdateRequestSchema.parse(req.body)

    const result = await notificationService.updateArchivedStatusAsync(userId, request)

    const response: NotificationStatusUpdateResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async updateAllReadStatusAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const request = ReadStatusUpdateAllRequestSchema.parse(req.body)

    const result = await notificationService.updateAllReadStatusAsync(userId, request)

    const response: NotificationStatusUpdateResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }

  @AsyncHandler
  public async updateAllArchivedStatusAsync(req: Request, res: Response) {
    const userId = req.headers[HEADERS.AUTH_ID] as string
    const request = ArchivedStatusUpdateAllRequestSchema.parse(req.body)

    const result = await notificationService.updateAllArchivedStatusAsync(userId, request)

    const response: NotificationStatusUpdateResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }
}

export default new NotificationController()
