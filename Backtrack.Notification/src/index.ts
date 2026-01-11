// MUST be first - load environment variables before anything else
import './configs/env-loader';

import http from 'http';
import ENV from '@src/common/constants/ENV';
import app from './server';
import { connectDatabase } from './configs/database';
import { startConsumers, stopConsumers } from './messaging/consumer-manager';
import logger from '@src/utils/logger';

// Connect to database
connectDatabase();

// Start RabbitMQ consumers
startConsumers().catch((error: unknown) => {
  logger.error('Failed to start RabbitMQ consumers:', error);
  logger.warn('Server will continue without message consumers');
});

const httpServer = http.createServer(app);

// Start server
httpServer.listen(ENV.Port, () => {
  logger.info(`Server started on port ${ENV.Port}`);
});

// Graceful shutdown
process.on('SIGINT', async () => {
  logger.info('Received SIGINT, shutting down gracefully...');
  await stopConsumers();
  httpServer.close(() => {
    logger.info('Server closed');
  });
});

process.on('SIGTERM', async () => {
  logger.info('Received SIGTERM, shutting down gracefully...');
  await stopConsumers();
  httpServer.close(() => {
    logger.info('Server closed');
  });
});
