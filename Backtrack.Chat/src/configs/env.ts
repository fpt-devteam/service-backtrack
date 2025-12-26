import ENV from '@src/common/constants/ENV';

export const env = {
  RABBITMQ_URL: ENV.RabbitMQ.Url,
  RABBITMQ_EXCHANGE: ENV.RabbitMQ.Exchange,
  RABBITMQ_USER_SYNC_QUEUE: ENV.RabbitMQ.UserSyncQueue,
};
