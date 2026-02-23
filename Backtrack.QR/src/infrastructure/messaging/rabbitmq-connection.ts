import amqp from 'amqplib';
import * as logger from '@/src/shared/core/logger.js';
import { env } from '@/src/infrastructure/configs/env.js';

type Connection = Awaited<ReturnType<typeof amqp.connect>>;
type Channel = Awaited<ReturnType<Connection['createChannel']>>;

let connection: Connection | null = null;
let channel: Channel | null = null;
const MAX_RETRIES = 5;
const RETRY_DELAY_MS = 5000;

export async function connectToRabbitMQ(retryCount = 0): Promise<Connection> {
  try {
    if (connection) {
      return connection;
    }

    const RABBITMQ_URL = env.RABBITMQ_URL;

    logger.info(`Connecting to RabbitMQ at ${RABBITMQ_URL}...`);
    connection = await amqp.connect(RABBITMQ_URL);

    connection.on('error', (err) => {
      logger.error('RabbitMQ connection error:', { error: String(err) });
      connection = null;
      channel = null;
    });

    connection.on('close', () => {
      logger.warn('RabbitMQ connection closed. Attempting to reconnect...');
      connection = null;
      channel = null;
      setTimeout(() => connectToRabbitMQ(), RETRY_DELAY_MS);
    });

    logger.info('Connected to RabbitMQ successfully');
    return connection;
  } catch (error) {
    logger.error(`Failed to connect to RabbitMQ (attempt ${retryCount + 1}/${MAX_RETRIES}):`, { error: String(error) });

    if (retryCount < MAX_RETRIES) {
      logger.info(`Retrying in ${RETRY_DELAY_MS / 1000} seconds...`);
      await new Promise(resolve => setTimeout(resolve, RETRY_DELAY_MS));
      return connectToRabbitMQ(retryCount + 1);
    } else {
      throw new Error(`Failed to connect to RabbitMQ after ${MAX_RETRIES} attempts`);
    }
  }
}

export async function createChannel(): Promise<Channel> {
  try {
    if (channel) {
      return channel;
    }

    logger.info('Creating RabbitMQ channel...');
    const conn = await connectToRabbitMQ();
    channel = await conn.createChannel();

    channel.on('error', (err) => {
      logger.error('RabbitMQ channel error:', { error: String(err) });
      channel = null;
    });

    channel.on('close', () => {
      logger.warn('RabbitMQ channel closed');
      channel = null;
    });

    logger.info('Created RabbitMQ channel successfully');
    return channel;
  } catch (error) {
    logger.error('Failed to create RabbitMQ channel:', { error: String(error) });
    throw error;
  }
}

export async function closeConnection(): Promise<void> {
  try {
    if (channel) {
      await channel.close();
      channel = null;
      logger.info('RabbitMQ channel closed');
    }

    if (connection) {
      await connection.close();
      connection = null;
      logger.info('RabbitMQ connection closed');
    }
  } catch (error) {
    logger.error('Error closing RabbitMQ connection:', { error: String(error) });
  }
}

export function getConnection(): Connection | null {
  return connection;
}

export function getChannel(): Channel | null {
  return channel;
}
