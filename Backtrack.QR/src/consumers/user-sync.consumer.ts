import { ConsumeMessage } from 'amqplib';
import { createChannel } from '@/src/messaging/rabbitmq-connection.js';
import * as logger from '@/src/utils/logger.js';
import { UserCreatedEvent, UserUpdatedEvent } from '@/src/contracts/events/user-events.js';
import { EventTopics } from '@/src/contracts/events/event-topics.js';
import * as userRepository from '@/src/repositories/user.repository.js';
import { env } from '@/src/configs/env.js';

export async function startUserSyncConsumer(): Promise<void> {
    try {
        const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
        const QUEUE_NAME = env.RABBITMQ_USER_SYNC_QUEUE;

        const channel = await createChannel();

        // Assert exchange and queue
        await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
        await channel.assertQueue(QUEUE_NAME, { durable: true });

        // Bind queue to exchange with routing key pattern
        await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, 'user.#');

        logger.info(`User sync consumer started. Listening to queue: ${QUEUE_NAME}`);

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
                    const event: UserCreatedEvent = JSON.parse(content);
                    await handleUserCreated(event);
                } else if (routingKey === EventTopics.User.Updated) {
                    const event: UserUpdatedEvent = JSON.parse(content);
                    await handleUserUpdated(event);
                } else {
                    logger.warn(`Unknown routing key: ${routingKey}`);
                }

                // Acknowledge successful processing
                channel.ack(msg);
                logger.info(`Successfully processed message with routing key: ${routingKey}`);
            } catch (error) {
                logger.error(`Error processing message with routing key ${routingKey}:`, { error: String(error) });
                // Negative acknowledge with requeue for transient errors
                channel.nack(msg, false, true);
            }
        });
    } catch (error) {
        logger.error('Failed to start user sync consumer:', { error: String(error) });
        throw error;
    }
}

async function handleUserCreated(event: UserCreatedEvent): Promise<void> {
    logger.info(`Handling UserCreated event for user ${event.Id}`);

    // Check if user already exists (idempotency)
    const existingResult = await userRepository.getById(event.Id);

    if (existingResult.success && existingResult.value) {
        logger.warn(`User ${event.Id} already exists. Skipping creation.`);
        return;
    }

    // Create user
    const createResult = await userRepository.create({
        _id: event.Id,
        email: event.Email,
        displayName: event.DisplayName,
        createdAt: new Date(event.CreatedAt),
        syncedAt: new Date()
    });

    if (!createResult.success) {
        logger.error(`Failed to create user ${event.Id}:`, createResult.error);
        throw new Error(`Failed to create user: ${createResult.error.message}`);
    }

    logger.info(`Successfully synced user ${event.Id} (created)`);
}

async function handleUserUpdated(event: UserUpdatedEvent): Promise<void> {
    logger.info(`Handling UserUpdated event for user ${event.Id}`);

    // Check if user exists
    const existingResult = await userRepository.getById(event.Id);

    if (!existingResult.success || !existingResult.value) {
        // User doesn't exist yet (out-of-order message), create it
        logger.warn(`User ${event.Id} not found. Creating user from update event.`);
        const createResult = await userRepository.create({
            _id: event.Id,
            email: event.Email || '',
            displayName: event.DisplayName,
            createdAt: new Date(event.UpdatedAt),
            syncedAt: new Date()
        });

        if (!createResult.success) {
            logger.error(`Failed to create user from update event ${event.Id}:`, createResult.error);
            throw new Error(`Failed to create user: ${createResult.error.message}`);
        }
        return;
    }

    // Update user
    const updateData: any = {
        updatedAt: new Date(event.UpdatedAt),
        syncedAt: new Date()
    };

    if (event.Email) updateData.email = event.Email;
    if (event.DisplayName !== undefined) updateData.displayName = event.DisplayName;

    const updateResult = await userRepository.update(event.Id, updateData);

    if (!updateResult.success) {
        logger.error(`Failed to update user ${event.Id}:`, updateResult.error);
        throw new Error(`Failed to update user: ${updateResult.error.message}`);
    }

    logger.info(`Successfully synced user ${event.Id} (updated)`);
}
