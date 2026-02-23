import winston from 'winston';
/**
 * Custom format for console output
 */
const consoleFormat = winston.format.combine(
  winston.format.timestamp({ format: 'YYYY-MM-DD HH:mm:ss' }),
  winston.format.colorize(),
  winston.format.printf(({ timestamp, level, message, correlationId, ...meta }) => {
    const correlation = correlationId ? `[${correlationId}]` : '';
    const metaStr = Object.keys(meta).length ? `\n${JSON.stringify(meta, null, 2)}` : '';
    return `[${timestamp}] ${level} ${correlation} ${message}${metaStr}`;
  })
);

/**
 * Create Winston logger instance
 */
const winstonLogger = winston.createLogger({
  level: process.env.LOG_LEVEL || 'info',
  format: winston.format.combine(
    winston.format.timestamp(),
    winston.format.errors({ stack: true }),
    winston.format.json()
  ),
  transports: [
    new winston.transports.Console({
      format: consoleFormat
    })
  ]
});

/**
 * Pure logging functions
 */
export const info = (message: string, meta?: object): void => {
  winstonLogger.info(message, meta);
};

export const warn = (message: string, meta?: object): void => {
  winstonLogger.warn(message, meta);
};

export const error = (message: string, meta?: object): void => {
  winstonLogger.error(message, meta);
};

export const debug = (message: string, meta?: object): void => {
  winstonLogger.debug(message, meta);
};

/**
 * Create logger with correlation ID (pure function factory)
 */
export const withCorrelationId = (correlationId: string) => ({
  info: (message: string, meta?: object) => info(message, { ...meta, correlationId }),
  warn: (message: string, meta?: object) => warn(message, { ...meta, correlationId }),
  error: (message: string, meta?: object) => error(message, { ...meta, correlationId }),
  debug: (message: string, meta?: object) => debug(message, { ...meta, correlationId })
});

export default winstonLogger;
