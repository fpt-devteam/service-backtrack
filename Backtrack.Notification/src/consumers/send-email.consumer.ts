import { Channel, ConsumeMessage } from 'amqplib'
import { createChannel } from '@src/messaging/rabbitmq-connection'
import { InvitationCreatedEvent } from '@src/contracts/events/invitation-event'
import { EventTopics } from '@src/contracts/events/event-topics'
import ENV from '@src/common/constants/ENV'
import logger from '@src/utils/logger'

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
  // Implement the logic to handle the invitation created event, such as sending an email
  // log all properties of the event for debugging purposes
  logger.info('Handling InvitationCreatedEvent:', {
    InvitationId: event.InvitationId,
    Email: event.Email,
    OrganizationName: event.OrganizationName,
    InviterName: event.InviterName,
    Role: event.Role,
    ExpiredTime: event.ExpiredTime,
    EventTimestamp: event.EventTimestamp,
  })
}
