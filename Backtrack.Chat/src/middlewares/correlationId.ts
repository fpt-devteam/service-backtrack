import { Request, Response, NextFunction } from 'express';
import { randomUUID } from 'crypto';

/**
 * Middleware to generate and attach correlationId to every request
 */
export const correlationIdMiddleware = (
  req: Request,
  res: Response,
  next: NextFunction,
) => {
  // Use existing correlationId from client or generate new one
  const correlationId =
    (req.headers['x-correlation-id'] as string) || randomUUID();

  // Attach to request headers for use in controllers and error handler
  req.headers['x-correlation-id'] = correlationId;

  // Optionally add to response headers for debugging
  res.setHeader('X-Correlation-Id', correlationId);

  next();
};
