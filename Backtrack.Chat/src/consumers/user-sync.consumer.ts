import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from '@src/messaging/rabbitmq-connection';
import {
  UserUpsertedEvent,
} from '@src/contracts/events/user-events';
import { EventTopics } from '@src/contracts/events/event-topics';
import { userRepository } from '@src/repositories';
import ENV from '@src/common/constants/ENV';
import logger from '@src/utils/logger';
import { parseUserGlobalRole, UserGlobalRole } from '@src/models/user.model';

const EXCHANGE_NAME = ENV.RabbitMQ.Exchange;
const QUEUE_NAME = ENV.RabbitMQ.UserSyncQueue;
const BINDING_PATTERN = 'user.#';

export async function startUserSyncConsumer(): Promise<void> {
  const channel = await setupChannel();
  await channel.consume(QUEUE_NAME, (msg) => processMessage(channel, msg));
}

async function setupChannel(): Promise<Channel> {
  try {
    const channel = await createChannel();
    await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
    await channel.assertQueue(QUEUE_NAME, { durable: true });
    await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN);
    logger.info(`User sync consumer started. Listening to queue: ${QUEUE_NAME}`);

    return channel;
  } catch (error) {
    logger.error('Failed to start user sync consumer:', { error: String(error) });
    throw error;
  }
}

async function processMessage(channel: Channel, msg: ConsumeMessage | null): Promise<void> {
  if (!msg) {
    return;
  }

  const routingKey = msg.fields.routingKey;
  const content = msg.content.toString();

  try {
    if (routingKey === EventTopics.User.Upserted) {
      const event: UserUpsertedEvent = JSON.parse(content);
      await handleUserUpsert(event);
    } else {
      logger.warn(`Unknown routing key: ${routingKey}`);
    }
    channel.ack(msg);
  } catch (error) {
    logger.error(`Error processing message with routing key ${routingKey}:`, { error: String(error) });
    channel.nack(msg, false, true);
  }
}

async function handleUserUpsert(event: UserUpsertedEvent): Promise<void> {
  await userRepository.upsertAsync({
    _id: event.Id,
    email: event.Email,
    displayName: event.DisplayName,
    globalRole: parseUserGlobalRole(event.GlobalRole) ?? UserGlobalRole.Customer,
    createdAt: new Date(event.CreatedAt),
    syncedAt: new Date(),
  });
}
