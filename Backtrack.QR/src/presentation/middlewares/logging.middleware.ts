
import { Request, Response, NextFunction } from 'express';
import * as logger from '@/src/shared/utils/logger.js';

export const loggingMiddleware = (req: Request, res: Response, next: NextFunction): void => {
    const correlationId = req.correlationId || 'unknown';
    const startTime = Date.now();

    // Log incoming request
    logger.info('Incoming request', {
        correlationId,
        method: req.method,
        path: req.path,
        query: req.query
    });

    // Log response when finished
    res.on('finish', () => {
        const duration = Date.now() - startTime;

        if (res.statusCode >= 400) {
            logger.warn('Request completed with error', {
                correlationId,
                method: req.method,
                path: req.path,
                statusCode: res.statusCode,
                duration: `${duration}ms`
            });
        } else {
            logger.info('Request completed', {
                correlationId,
                method: req.method,
                path: req.path,
                statusCode: res.statusCode,
                duration: `${duration}ms`
            });
        }
    });

    next();
};
