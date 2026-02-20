import { Request, Response, NextFunction } from 'express';
import { getHeader, HeaderNames } from '@/src/presentation/utils/http-headers.util.js';

declare global {
  namespace Express {
    interface Request {
      correlationId?: string;
    }
  }
}

export const correlationMiddleware = (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  const correlationId = getHeader(req, HeaderNames.CorrelationId) ?? "missing-correlation-id";
  req.correlationId = correlationId;

  const originalJson = res.json.bind(res);
  res.json = (body: unknown) => {
    return originalJson({ ...(body as object), correlationId });
  };
  next();
};
