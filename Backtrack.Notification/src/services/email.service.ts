import { Resend } from 'resend'
import logger from '@src/utils/logger'
import { EmailSendRequest } from '@src/contracts/requests/email.request'
import { EmailSendResult, EmailVerifyResult } from '@src/contracts/responses/email.response'
import ENV from '@src/common/constants/ENV'

const EmailConfig = {
  apiKey: ENV.Email.ResendApiKey,
  defaultFrom: ENV.Email.DefaultFrom,
}

class EmailService {
  private readonly resend: Resend

  constructor() {
    this.resend = new Resend(EmailConfig.apiKey)

    logger.info('Resend email client initialized', {
      sender: EmailConfig.defaultFrom,
      mode: 'HTTP API'
    })
  }

  public async sendEmailAsync(request: EmailSendRequest) {
    try {
      logger.info('Sending email via Resend API', {
        to: request.to,
        subject: request.subject,
      })

      const { data, error } = await this.resend.emails.send({
        from: EmailConfig.defaultFrom,
        to: request.to,
        subject: request.subject,
        text: request.text,
        html: request.html,
      });

      if (error) {
        logger.error('Resend API returned an error', {
          error,
          to: request.to
        })
        return { sent: false } as EmailSendResult
      }

      logger.info('Email sent successfully', {
        messageId: data?.id,
        to: request.to,
      })

      return { sent: true } as EmailSendResult
    } catch (error) {

      logger.error('Failed to send email (Exception)', {
        error,
        to: request.to,
        subject: request.subject,
      })

      return { sent: false } as EmailSendResult
    }
  }
}

export default new EmailService()