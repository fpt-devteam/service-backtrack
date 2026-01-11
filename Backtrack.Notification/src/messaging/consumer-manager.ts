import { connectToRabbitMQ, closeConnection } from './rabbitmq-connection';
import { startUserSyncConsumer } from '@src/consumers/user-sync.consumer';

import logger from '@src/utils/logger';

export async function startConsumers(): Promise<void> {
  try {
    logger.info('Starting message consumers...');

    await connectToRabbitMQ();

    await startUserSyncConsumer();

    logger.info('All message consumers started successfully');
  } catch (error) {
    logger.error('Failed to start message consumers:', String(error));
    throw error;
  }
}

export async function stopConsumers(): Promise<void> {
  try {
    logger.info('Stopping message consumers...');

    await closeConnection();

    logger.info('All message consumers stopped successfully');
  } catch (error) {
    logger.error('Error stopping message consumers:', String(error));
  }
}
