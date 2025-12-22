import { Request, Response, NextFunction } from 'express';
import { AppError } from '@src/common/errors';
import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES';

export const errorHandler = (
  err: Error,
  req: Request,
  res: Response,
  _next: NextFunction,
) => {
  const correlationId = req.headers['x-correlation-id'] as string;

  // Handle operational errors (AppError instances)
  if (err instanceof AppError) {
    const response = {
      success: false,
      error: {
        code: err.constructor.name,
        message: err.message,

      },
      correlationId,
    };

    return res.status(err.statusCode).json(response);
  }

  
  // Handle validation errors
  return res.status(HTTP_STATUS_CODES.InternalServerError).json({
    success: false,
    error: {
      code: 'InternalServerError',
      message: 'An unexpected error occurred',
    },
    correlationId,
  });
};
