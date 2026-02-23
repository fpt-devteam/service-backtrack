import { Request, Response, NextFunction } from 'express';
import * as logger from '@/src/shared/core/logger.js';

export const loggingMiddleware = (req: Request, res: Response, next: NextFunction): void => {
  const correlationId = req.correlationId;
  const startTime = Date.now();

  logger.info('Incoming request', {
    correlationId,
    method: req.method,
    path: req.path,
    query: req.query
  });

  res.on('finish', () => {
    const duration = Date.now() - startTime;
    const message = res.statusCode >= 400 ? 'Request completed with error' : 'Request completed';
    logger.warn(message, {
      correlationId,
      method: req.method,
      path: req.path,
      statusCode: res.statusCode,
      duration: `${duration}ms`
    });
  });

  next();
};
