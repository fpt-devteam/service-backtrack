import nodemailer, { Transporter, SendMailOptions } from 'nodemailer'
import logger from '@src/utils/logger'
import { EmailSendRequest } from '@src/contracts/requests/email.request'
import { EmailSendResult, EmailVerifyResult } from '@src/contracts/responses/email.response'
import ENV from '@src/common/constants/ENV'

const EmailConfig = {
  service: 'gmail',
  port: 465,
  secure: true,
  host: 'smtp.gmail.com',
  auth: {
    user: ENV.Email.User,
    pass: ENV.Email.Pass,
  },
  tls: {
    rejectUnauthorized: true,
  },
  family: 4,
}

class EmailService {
  private readonly transporter: Transporter

  constructor() {
    this.transporter = nodemailer.createTransport(EmailConfig);

    logger.info('Email transporter connection verified')

    logger.info('Email transporter initialized', {
      host: EmailConfig.host,
      user: EmailConfig.auth.user,
    })
  }

  public async sendEmailAsync(request: EmailSendRequest) {
    try {
      const isConnected = await this.transporter.verify()
      if (!isConnected) {
        logger.error('Email transporter is not connected')
        const result: EmailSendResult = {
          sent: false,
        }
        return result
      }

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