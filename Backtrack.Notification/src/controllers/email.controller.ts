import { Request, Response } from 'express'
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AsyncHandler } from '@src/decorators/async-handler'
import emailService from '@src/services/email.service'
import {
  EmailSendRequestSchema,
} from '@src/contracts/requests/email.request'
import {
  EmailSendResponse,
  EmailVerifyResponse,
} from '@src/contracts/responses/email.response'

export class EmailController {
  @AsyncHandler
  public async sendEmailAsync(req: Request, res: Response) {
    const payload = EmailSendRequestSchema.parse(req.body)
    const result = await emailService.sendEmailAsync(payload)

    const response: EmailSendResponse = {
      success: true,
      data: result,
    }

    return res.status(HTTP_STATUS_CODES.Ok).json(response)
  }
}

export default new EmailController()
