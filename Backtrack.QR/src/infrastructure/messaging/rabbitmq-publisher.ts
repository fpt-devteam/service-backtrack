import { createChannel } from './rabbitmq-connection.js';
import { env } from '@/src/shared/configs/env.js';
import * as logger from '@/src/shared/utils/logger.js';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;

export interface PublishOptions {
    persistent?: boolean;
}

export async function publishMessage<T>(
    routingKey: string,
    message: T,
    options: PublishOptions = {}
): Promise<boolean> {
    try {
        const channel = await createChannel();

        await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });

        const messageBuffer = Buffer.from(JSON.stringify(message));

        const published = channel.publish(
            EXCHANGE_NAME,
            routingKey,
            messageBuffer,
            {
                persistent: options.persistent ?? true,
                contentType: 'application/json',
                timestamp: Date.now(),
            }
        );

        if (published) {
            logger.info('Message published successfully', {
                exchange: EXCHANGE_NAME,
                routingKey,
                messageSize: messageBuffer.length,
            });
        } else {
            logger.warn('Message was not published (channel buffer full)', {
                exchange: EXCHANGE_NAME,
                routingKey,
            });
        }

        return published;
    } catch (error) {
        logger.error('Failed to publish message', {
            exchange: EXCHANGE_NAME,
            routingKey,
            error: String(error),
        });
        throw error;
    }
}
