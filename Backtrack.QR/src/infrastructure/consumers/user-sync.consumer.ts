import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from '@/src/infrastructure/messaging/rabbitmq-connection.js';
import * as logger from '@/src/shared/utils/logger.js';
import { UserEnsureExistEvent } from '@/src/shared/contracts/events/user-events.js';
import { EventTopics } from '@/src/shared/contracts/events/event-topics.js';
import { userRepository } from '@/src/infrastructure/repositories/user.repository.js';
import { env } from '@/src/shared/configs/env.js';
import { parseUserGlobalRole, UserGlobalRole } from '@/src/infrastructure/database/models/user.model.js';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
const QUEUE_NAME = env.RABBITMQ_USER_SYNC_QUEUE;
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
    if (routingKey === EventTopics.User.EnsureExist) {
      const event: UserEnsureExistEvent = JSON.parse(content);
      await handleUserEnsureExist(event);
    } else {
      logger.warn(`Unknown routing key: ${routingKey}`);
    }
    channel.ack(msg);
  } catch (error) {
    logger.error(`Error processing message with routing key ${routingKey}:`, { error: String(error) });
    channel.nack(msg, false, true);
  }
}

async function handleUserEnsureExist(user: UserEnsureExistEvent): Promise<void> {
  await userRepository.ensureExist({
    _id: user.Id,
    email: user.Email,
    displayName: user.DisplayName,
    avatarUrl: user.AvatarUrl ?? null,
    globalRole: parseUserGlobalRole(user.GlobalRole) ?? UserGlobalRole.Customer,
    createdAt: new Date(user.CreatedAt),
    syncedAt: new Date()
  });
}
