import { ConsumeMessage } from 'amqplib';
import { createChannel } from '@src/messaging/rabbitmq-connection';
import {
  UserCreatedEvent,
  UserUpdatedEvent,
} from '@src/contracts/events/user-events';
import { EventTopics } from '@src/contracts/events/event-topics';
import { userRepository } from '@src/repositories';
import ENV from '@src/common/constants/ENV';
import logger from '@src/utils/logger';

export async function startUserSyncConsumer(): Promise<void> {
  try {
    const EXCHANGE_NAME = ENV.RabbitMQ.Exchange;
    const QUEUE_NAME = ENV.RabbitMQ.UserSyncQueue;

    const channel = await createChannel();

    // Assert exchange and queue
    await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
    await channel.assertQueue(QUEUE_NAME, { durable: true });

    // Bind queue to exchange with routing key pattern
    await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, 'user.#');

    logger.info(
      `User sync consumer started. Listening to queue: ${QUEUE_NAME}`,
    );

    // Start consuming messages
    await channel.consume(QUEUE_NAME, async (msg: ConsumeMessage | null) => {
      if (!msg) {
        return;
      }

      const routingKey = msg.fields.routingKey;
      const content = msg.content.toString();

      try {
        logger.info(`Received message with routing key: ${routingKey}`);

        if (routingKey === EventTopics.User.Created) {
          // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
          const event: UserCreatedEvent = JSON.parse(content);
          await handleUserCreated(event);
        } else if (routingKey === EventTopics.User.Updated) {
          // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
          const event: UserUpdatedEvent = JSON.parse(content);
          await handleUserUpdated(event);
        } else {
          logger.warn(`Unknown routing key: ${routingKey}`);
        }

        // Acknowledge successful processing
        channel.ack(msg);
        logger.info(
          `Successfully processed message with routing key: ${routingKey}`,
        );
      } catch (error) {
        logger.error(
          `Error processing message with routing key ${routingKey}:`,
          String(error),
        );
        // Negative acknowledge with requeue for transient errors
        channel.nack(msg, false, true);
      }
    });
  } catch (error) {
    logger.error('Failed to start user sync consumer:', String(error));
    throw error;
  }
}

async function handleUserCreated(event: UserCreatedEvent): Promise<void> {
  logger.info(`Handling UserCreated event for user ${event.Id}`);

  // Check if user already exists (idempotency)
  const existingUser = await userRepository.getByIdAsync(event.Id);

  if (existingUser) {
    logger.warn(`User ${event.Id} already exists. Skipping creation.`);
    return;
  }

  // Create user
  await userRepository.createAsync({
    _id: event.Id,
    email: event.Email,
    displayName: event.DisplayName,
    avatarUrl: event.AvatarUrl,
    createdAt: new Date(event.CreatedAt),
    syncedAt: new Date(),
  });

  logger.info(`Successfully synced user ${event.Id} (created)`);
}

async function handleUserUpdated(event: UserUpdatedEvent): Promise<void> {
  logger.info(`Handling UserUpdated event for user ${event.Id}`);

  // Check if user exists
  const existingUser = await userRepository.getByIdAsync(event.Id);

  if (!existingUser) {
    // User doesn't exist yet (out-of-order message), create it
    logger.warn(
      `User ${event.Id} not found. Creating user from update event.`,
    );
    await userRepository.createAsync({
      _id: event.Id,
      email: event.Email ?? '',
      displayName: event.DisplayName,
      avatarUrl: event.AvatarUrl,
      createdAt: new Date(event.UpdatedAt),
      syncedAt: new Date(),
    });
    return;
  }

  // Update user
  const updateData: {
    email?: string,
    displayName?: string,
    avatarUrl?: string | null,
    updatedAt: Date,
    syncedAt: Date,
  } = {
    updatedAt: new Date(event.UpdatedAt),
    syncedAt: new Date(),
  };

  if (event.Email) updateData.email = event.Email;
  if (
    event.DisplayName !== undefined
  ) updateData.displayName = event.DisplayName;
  if (event.AvatarUrl !== undefined) updateData.avatarUrl = event.AvatarUrl;

  await userRepository.updateAsync(event.Id, updateData);

  logger.info(`Successfully synced user ${event.Id} (updated)`);
}
