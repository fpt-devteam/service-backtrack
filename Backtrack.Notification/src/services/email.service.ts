import nodemailer, { Transporter, SendMailOptions } from 'nodemailer'
import logger from '@src/utils/logger'
import { EmailSendRequest } from '@src/contracts/requests/email.request'
import { EmailSendResult, EmailVerifyResult } from '@src/contracts/responses/email.response'
import ENV from '@src/common/constants/ENV'

const EmailConfig = {
  service: ENV.Email.Service,
  port: 465,
  secure: true,
  auth: {
    user: ENV.Email.User,
    pass: ENV.Email.Pass,
  },
}

class EmailService {
  private readonly transporter: Transporter

  constructor() {
    this.transporter = nodemailer.createTransport({
      service: EmailConfig.service,
      auth: {
        user: EmailConfig.auth.user,
        pass: EmailConfig.auth.pass,
      },
    })

    logger.info('Email transporter initialized', {
      service: EmailConfig.service,
      user: EmailConfig.auth.user,
    })
  }

  public async sendEmailAsync(request: EmailSendRequest) {
    try {
      const mailOptions: SendMailOptions = {
        from: EmailConfig.auth.user,
        to: request.to,
        subject: request.subject,
        text: request.text,
        html: request.html,
        cc: request.cc,
        bcc: request.bcc,
      }

      logger.info('Sending email', {
        to: request.to,
        subject: request.subject,
      })

      const info = await this.transporter.sendMail(mailOptions)
      logger.info('Email sent successfully', {
        messageId: info.messageId,
        to: request.to,
      })

      const result: EmailSendResult = {
        sent: true,
      }

      return result
    } catch (error) {
      logger.error('Failed to send email', {
        error,
        to: request.to,
        subject: request.subject,
      })

      const result: EmailSendResult = {
        sent: false,
      }

      return result
    }
  }

  public async verifyConnectionAsync() {
    try {
      await this.transporter.verify()
      logger.info('Email transporter connection verified')

      const result: EmailVerifyResult = {
        connected: true,
      }
      return result
    } catch (error) {
      logger.error('Email transporter verification failed', error)

      const result: EmailVerifyResult = {
        connected: false,
      }
      return result
    }
  }
}

export default new EmailService()
