import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from './rabbitmq-connection';
import { env } from '@/config/environment';
import logger from '@/utils/logger';
import * as userService from '@/services/user.service';
import { EventTopics } from '@/events/event-topic';
import { UserEnsureExistEvent } from '@/events/user-event';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
const QUEUE_NAME = env.RABBITMQ_USER_SYNC_QUEUE;
const BINDING_PATTERN = 'user.#';

const DLX_EXCHANGE_NAME = `${EXCHANGE_NAME}.dlx`;
const DLQ_NAME = `${QUEUE_NAME}.dlq`;
const MAX_RETRIES = 3;

// In-memory retry tracking keyed by messageId (set by publisher) or content hash.
// Cleared on ack or final nack.
const retryCounts = new Map<string, number>();

function getMessageKey(msg: ConsumeMessage): string {
  return msg.properties.messageId ?? `${msg.fields.routingKey}:${msg.content.toString()}`;
}

export async function startUserSyncConsumer(): Promise<void> {
  const channel = await setupChannel();
  await channel.consume(QUEUE_NAME, (msg) => processMessage(channel, msg));
}

async function setupChannel(): Promise<Channel> {
  try {
    const channel = await createChannel();

    await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });

    // Dead-letter exchange (fanout) and its queue — receives poison messages.
    await channel.assertExchange(DLX_EXCHANGE_NAME, 'fanout', { durable: true });
    await channel.assertQueue(DLQ_NAME, { durable: true });
    await channel.bindQueue(DLQ_NAME, DLX_EXCHANGE_NAME, '');

    // Main queue: configure DLX so nack(false, false) routes to DLQ.
    // NOTE: if this queue already exists without x-dead-letter-exchange, delete
    // it first so RabbitMQ can recreate it with the new argument.
    await channel.assertQueue(QUEUE_NAME, {
      durable: true,
      arguments: { 'x-dead-letter-exchange': DLX_EXCHANGE_NAME },
    });
    await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN);

    logger.info(`Bound queue ${QUEUE_NAME} to exchange ${EXCHANGE_NAME} with pattern ${BINDING_PATTERN} successfully`);
    logger.info(`User sync consumer started. Listening to queue: ${QUEUE_NAME} successfully`);

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
  logger.info(`Received message with routing key ${routingKey}: ${content}`);

  try {
    if (routingKey === EventTopics.User.EnsureExist) {
      const event: UserEnsureExistEvent = JSON.parse(content);
      await handleUserEnsureExist(event);
    } else {
      logger.warn(`Unknown routing key: ${routingKey}`);
    }
    const key = getMessageKey(msg);
    retryCounts.delete(key);
    channel.ack(msg);
    logger.info(`Message with routing key ${routingKey} processed successfully`);
  } catch (error) {
    const key = getMessageKey(msg);
    const attempt = (retryCounts.get(key) ?? 0) + 1;
    logger.error(`Error processing message with routing key ${routingKey} (attempt ${attempt}/${MAX_RETRIES}):`, { error: String(error) });

    if (attempt >= MAX_RETRIES) {
      retryCounts.delete(key);
      logger.error(`Max retries reached for routing key ${routingKey}, routing to DLQ`);
      channel.nack(msg, false, false); // no requeue → DLQ via DLX
    } else {
      retryCounts.set(key, attempt);
      channel.nack(msg, false, true); // requeue for retry
    }
  }
}

async function handleUserEnsureExist(user: UserEnsureExistEvent): Promise<void> {
console.log('Ensuring user exists:', user);
  await userService.ensureUserExists({
    id: user.Id,
    email: user.Email || undefined, // empty string → undefined → email field left untouched in DB
    displayName: user.DisplayName,
    avatarUrl: user.AvatarUrl,
    globalRole: user.GlobalRole,
    providerCustomerId: undefined,
    subscriptionStatus: undefined,
  });
}
