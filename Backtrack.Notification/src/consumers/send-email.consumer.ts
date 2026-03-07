import { Channel, ConsumeMessage } from 'amqplib'
import { createChannel } from '@src/messaging/rabbitmq-connection'
import { InvitationCreatedEvent } from '@src/contracts/events/invitation-event'
import { EventTopics } from '@src/contracts/events/event-topics'
import ENV from '@src/common/constants/ENV'
import logger from '@src/utils/logger'
import emailService from '@src/services/email.service'
import fs from 'fs'
import path from 'path'

const EXCHANGE_NAME = ENV.RabbitMQ.Exchange
const QUEUE_NAME = ENV.RabbitMQ.SendEmailQueue
const BINDING_PATTERN = 'invitation.#'

export async function startSendEmailConsumer(): Promise<void> {
  const channel = await setupChannel()
  await channel.consume(QUEUE_NAME, (msg) => processMessage(channel, msg))
}

async function setupChannel(): Promise<Channel> {
  try {
    const channel = await createChannel()
    await channel.assertQueue(QUEUE_NAME, { durable: true })
    await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN)
    logger.info(`Send email consumer started. Listening to queue: ${QUEUE_NAME}`)

    return channel
  } catch (error) {
    logger.error('Failed to start send email consumer:', {
      error: String(error),
    })
    throw error
  }
}

async function processMessage(
  channel: Channel,
  msg: ConsumeMessage | null,
): Promise<void> {
  if (!msg) {
    return
  }

  const routingKey = msg.fields.routingKey
  const content = msg.content.toString()

  try {
    if (routingKey === EventTopics.Invitation.Created) {
      const event: InvitationCreatedEvent = JSON.parse(content)
      await handleInvitationCreated(event)
    } else {
      logger.warn(`Unknown routing key: ${routingKey}`)
    }
    channel.ack(msg)
  } catch (error) {
    logger.error(`Error processing message with routing key ${routingKey}:`, {
      error: String(error),
    })
    channel.nack(msg, false, true)
  }
}

async function handleInvitationCreated(event: InvitationCreatedEvent): Promise<void> {
  try {
    logger.info('Handling InvitationCreatedEvent:', {
      InvitationId: event.InvitationId,
      Email: event.Email,
      OrganizationName: event.OrganizationName,
    })

    const joinUrl = `${ENV.BacktrackConsoleWebDomain}/auth/signin-or-signup?code=${event.HashCode}&email=${encodeURIComponent(event.Email)}`

    // Calculate expiration hours
    const expiredTime = new Date(event.ExpiredTime)
    const now = new Date()
    const diffMs = expiredTime.getTime() - now.getTime()
    const diffHours = Math.max(0, Math.round(diffMs / (1000 * 60 * 60)))

    // Read and fill template
    // In dev: process.cwd()/src/templates/invitation-email.html
    // In prod: process.cwd()/dist/templates/invitation-email.html
    let templatePath = path.join(process.cwd(), 'src', 'templates', 'invitation-email.html')
    if (!fs.existsSync(templatePath)) {
      templatePath = path.join(process.cwd(), 'dist', 'templates', 'invitation-email.html')
    }

    if (!fs.existsSync(templatePath)) {
      // Fallback to relative path if process.cwd() is not as expected
      templatePath = path.join(__dirname, '..', 'templates', 'invitation-email.html')
    }

    let htmlContent = fs.readFileSync(templatePath, 'utf8')

    htmlContent = htmlContent
      .replace(/{{organizationName}}/g, event.OrganizationName)
      .replace(/{{joinUrl}}/g, joinUrl)
      .replace(/{{expireHours}}/g, diffHours.toString())

    // Send email
    await emailService.sendEmailAsync({
      to: event.Email,
      subject: `Invitation to join ${event.OrganizationName} on Backtrack`,
      html: htmlContent,
      text: `Hello, you have been invited to join the ${event.OrganizationName} group on Backtrack. Join here: ${joinUrl}`,
    })

    logger.info('Invitation email sent successfully', {
      Email: event.Email,
      InvitationId: event.InvitationId,
    })
  } catch (error) {
    logger.error('Error in handleInvitationCreated:', {
      error: String(error),
      InvitationId: event.InvitationId,
    })
    throw error
  }
}
