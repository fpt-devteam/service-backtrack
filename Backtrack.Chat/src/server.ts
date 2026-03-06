import 'dotenv/config';
import { createServer } from 'http';

import app from './app';
import { env } from '@/config/environment';
import logger from '@/utils/logger';
import { connectDatabase, disconnectDatabase } from '@/config/database';
import { initializeFirebase } from '@/config/firebase';
import { initializeWebSocket } from '@/config/websocket';
import { startConsumers, stopConsumers } from '@/messaging/consumer-manager';

const startServer = async (): Promise<void> => {
	try {
		await connectDatabase();

		// Initialize Firebase Admin for WebSocket JWT self-verification
		initializeFirebase();

		// Start RabbitMQ consumers
		startConsumers().catch((error: unknown) => {
			logger.error('Failed to start RabbitMQ consumers:', error);
			logger.warn('Server will continue without message consumers');
		});

		// Create HTTP server
		const httpServer = createServer(app);

		// Initialize WebSocket
		initializeWebSocket(httpServer);

		httpServer.listen(env.PORT, () => {
			logger.info(
				`Server running in ${env.NODE_ENV} mode at http://${env.HOST}:${env.PORT}`,
			);
			logger.info('WebSocket server ready for connections');
		});

		const shutdown = async (signal: string): Promise<void> => {
			logger.info(`${signal} received. Shutting down gracefully...`);

			httpServer.close(async () => {
				await stopConsumers();
				await disconnectDatabase();
				logger.info('Server closed.');
				process.exit(0);
			});

			setTimeout(() => {
				logger.error('Forced shutdown after timeout');
				process.exit(1);
			}, 10000);
		};

		process.on('SIGINT', () => shutdown('SIGINT'));
		process.on('SIGTERM', () => shutdown('SIGTERM'));
	} catch (error) {
		logger.error('Failed to start server:', error as any);
		process.exit(1);
	}
};

startServer();
