import logger from '@/utils/logger';
import { startUserSyncConsumer } from './user-sync.consumer';
import { startOrgSyncConsumer } from './org-sync.consumer';
import { closeConnection, connectToRabbitMQ } from './rabbitmq-connection';


export async function startConsumers(): Promise<void> {
  logger.info('Starting message consumers...');
  await connectToRabbitMQ();
  await startUserSyncConsumer();
  await startOrgSyncConsumer();
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
