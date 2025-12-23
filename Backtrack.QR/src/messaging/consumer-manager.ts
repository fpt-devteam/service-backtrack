import { connectToRabbitMQ, closeConnection } from './rabbitmq-connection.js';
import { startUserSyncConsumer } from '@/src/consumers/user-sync.consumer.js';
import * as logger from '@/src/utils/logger.js';

export async function startConsumers(): Promise<void> {
    try {
        logger.info('Starting message consumers...');

        // Connect to RabbitMQ
        await connectToRabbitMQ();

        // Start all consumers
        await startUserSyncConsumer();

        logger.info('All message consumers started successfully');
    } catch (error) {
        logger.error('Failed to start message consumers:', { error: String(error) });
        throw error;
    }
}

export async function stopConsumers(): Promise<void> {
    try {
        logger.info('Stopping message consumers...');

        // Close RabbitMQ connection (will also close all channels and consumers)
        await closeConnection();

        logger.info('All message consumers stopped successfully');
    } catch (error) {
        logger.error('Error stopping message consumers:', { error: String(error) });
    }
}
