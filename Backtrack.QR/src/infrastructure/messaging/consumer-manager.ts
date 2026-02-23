import { connectToRabbitMQ, closeConnection } from '@/src/infrastructure/messaging/rabbitmq-connection.js';
import { startUserSyncConsumer } from '@/src/infrastructure/messaging/user-sync.consumer.js';
import * as logger from '@/src/shared/core/logger.js';

export async function startConsumers(): Promise<void> {
  logger.info('Starting message consumers...');
  await connectToRabbitMQ();
  await startUserSyncConsumer();
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
