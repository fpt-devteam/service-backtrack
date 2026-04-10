import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from './rabbitmq-connection';
import { env } from '@/config/environment';
import logger from '@/utils/logger';
import * as orgService from '@/services/org.service';
import { EventTopics } from '@/events/event-topic';
import { OrgEnsureExistEvent } from '@/events/org-event';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
const QUEUE_NAME = env.RABBITMQ_ORG_SYNC_QUEUE;
const BINDING_PATTERN = 'org.#';

export async function startOrgSyncConsumer(): Promise<void> {
    const channel = await setupChannel();
    await channel.consume(QUEUE_NAME, (msg) => processMessage(channel, msg));
}

async function setupChannel(): Promise<Channel> {
    try {
        const channel = await createChannel();
        await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
        await channel.assertQueue(QUEUE_NAME, { durable: true });
        await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN);
        logger.info(`Bound queue ${QUEUE_NAME} to exchange ${EXCHANGE_NAME} with pattern ${BINDING_PATTERN} successfully`);
        logger.info(`Org sync consumer started. Listening to queue: ${QUEUE_NAME} successfully`);
        return channel;
    } catch (error) {
        logger.error('Failed to start org sync consumer:', { error: String(error) });
        throw error;
    }
}

async function processMessage(channel: Channel, msg: ConsumeMessage | null): Promise<void> {
    if (!msg) return;

    const routingKey = msg.fields.routingKey;
    const content = msg.content.toString();
    logger.info(`Received message with routing key ${routingKey}: ${content}`);

    try {
        if (routingKey === EventTopics.Org.EnsureExist) {
            const event: OrgEnsureExistEvent = JSON.parse(content);
            await handleOrgEnsureExist(event);
        } else {
            logger.warn(`Unknown routing key: ${routingKey}`);
        }
        channel.ack(msg);
        logger.info(`Message with routing key ${routingKey} processed successfully`);
    } catch (error) {
        logger.error(`Error processing message with routing key ${routingKey}:`, { error: String(error) });
        channel.nack(msg, false, true);
    }
}

async function handleOrgEnsureExist(event: OrgEnsureExistEvent): Promise<void> {
    await orgService.ensureOrgExists({
        id: event.Id,
        name: event.Name,
        slug: event.Slug,
        logoUrl: event.LogoUrl,
    });
}
