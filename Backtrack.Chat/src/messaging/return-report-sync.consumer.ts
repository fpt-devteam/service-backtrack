import { Channel, ConsumeMessage } from 'amqplib';
import { createChannel } from './rabbitmq-connection';
import { env } from '@/config/environment';
import logger from '@/utils/logger';
import { EventTopics } from '@/events/event-topic';
import { ReturnReportSyncEvent } from '@/events/return-report-event';
import { syncReturnReportHandover } from '@/services/return-report-sync.service';

const EXCHANGE_NAME = env.RABBITMQ_EXCHANGE;
const QUEUE_NAME = env.RABBITMQ_RETURN_REPORT_SYNC_QUEUE;
const BINDING_PATTERN = 'return-report.#';

export async function startReturnReportSyncConsumer(): Promise<void> {
    const channel = await setupChannel();
    await channel.consume(QUEUE_NAME, (msg) => processMessage(channel, msg));
}

async function setupChannel(): Promise<Channel> {
    try {
        const channel = await createChannel();
        await channel.assertExchange(EXCHANGE_NAME, 'topic', { durable: true });
        await channel.assertQueue(QUEUE_NAME, { durable: true });
        await channel.bindQueue(QUEUE_NAME, EXCHANGE_NAME, BINDING_PATTERN);
        logger.info(`Return report sync consumer started. Listening to queue: ${QUEUE_NAME}`);
        return channel;
    } catch (error) {
        logger.error('Failed to start return report sync consumer:', { error: String(error) });
        throw error;
    }
}

async function processMessage(channel: Channel, msg: ConsumeMessage | null): Promise<void> {
    if (!msg) return;

    const routingKey = msg.fields.routingKey;
    const content = msg.content.toString();
    logger.info(`Received message with routing key ${routingKey}: ${content}`);

    try {
        if (routingKey === EventTopics.ReturnReport.Synced) {
            const event: ReturnReportSyncEvent = JSON.parse(content);
            await handleReturnReportSync(event);
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

async function handleReturnReportSync(event: ReturnReportSyncEvent): Promise<void> {
    await syncReturnReportHandover(event);
}
