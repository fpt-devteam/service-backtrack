import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from '@/src/infrastructure/messaging/rabbitmq-connection.js';
import * as logger from '@/src/shared/utils/logger.js';
import { EventTopics } from '@/src/shared/contracts/events/event-topics.js';
import { env } from '@/src/shared/configs/env.js';
import type { QrGenerationRequestedEvent } from '@/src/shared/contracts/events/order-events.js';
import { processBatchQrGenerationAsync } from '@/src/domain/services/qr-code.service.js';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
const QUEUE_NAME = env.RABBITMQ_QR_GENERATION_QUEUE;
const BINDING_PATTERN = EventTopics.Order.QrGenerationRequested;

export async function startQrGenerationConsumer(): Promise<void> {
    const channel = await setupChannel();
    await channel.consume(QUEUE_NAME, (msg) => {
        processMessage(channel, msg).catch((error) => {
            logger.error('Unhandled error in QR generation consumer:', { error: String(error) });
        });
    });
}

async function setupChannel(): Promise<Channel> {
    try {
        const channel = await createChannel();
        await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
        await channel.assertQueue(QUEUE_NAME, { durable: true });
        await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN);

        // Set prefetch to process one message at a time for better reliability
        await channel.prefetch(1);

        logger.info(`QR generation consumer started. Listening to queue: ${QUEUE_NAME}`);
        return channel;
    } catch (error) {
        logger.error('Failed to start QR generation consumer:', { error: String(error) });
        throw error;
    }
}

async function processMessage(channel: Channel, msg: ConsumeMessage | null): Promise<void> {
    if (!msg) {
        return;
    }

    const routingKey = msg.fields.routingKey;
    const content = msg.content.toString();

    logger.info('QR generation consumer received message', { routingKey, content });

    try {
        if (routingKey === EventTopics.Order.QrGenerationRequested) {
            const event: QrGenerationRequestedEvent = JSON.parse(content);
            await processBatchQrGenerationAsync(event);
        } else {
            logger.warn(`Unknown routing key in QR generation consumer: ${routingKey}`);
        }
        channel.ack(msg);
    } catch (error) {
        logger.error(`Error processing QR generation message with routing key ${routingKey}:`, {
            error: String(error),
            content,
        });
        // Requeue the message for retry
        channel.nack(msg, false, true);
    }
}
