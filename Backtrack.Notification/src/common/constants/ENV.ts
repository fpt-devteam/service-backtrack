/******************************************************************************
                                 Setup
******************************************************************************/

function getEnv(key: string, defaultValue?: string): string {
  const value = process.env[key]
  if (!value && defaultValue === undefined) {
    throw new Error(`Environment variable ${key} is not defined`)
  }
  return value ?? defaultValue ?? ''
}

function getEnvNumber(key: string, defaultValue?: number): number {
  const value = process.env[key]
  if (!value) {
    if (defaultValue === undefined) {
      throw new Error(`Environment variable ${key} is not defined`)
    }
    return defaultValue
  }
  const num = parseInt(value, 10)
  if (isNaN(num)) {
    throw new Error(`Environment variable ${key} must be a number`)
  }
  return num
}

/******************************************************************************
                          Structured Export
******************************************************************************/

export default {
  NodeEnv: getEnv('NODE_ENV', 'development'),
  Port: getEnvNumber('PORT', 5000),
  MongodbConnectionstring: getEnv('MONGODB_CONNECTIONSTRING'),

  Pagination: {
    DefaultLimit: getEnvNumber('DEFAULT_PAGE_LIMIT', 20),
    MaxLimit: getEnvNumber('MAX_PAGE_LIMIT', 100),
  },

  RabbitMQ: {
    Url: getEnv('RABBITMQ_URL'),
    Exchange: getEnv('RABBITMQ_EXCHANGE'),
    UserSyncQueue: getEnv('RABBITMQ_USER_SYNC_QUEUE'),
    SendEmailQueue: getEnv('RABBITMQ_SEND_EMAIL_QUEUE'),
  },
} as const

