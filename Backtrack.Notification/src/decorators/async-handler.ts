import { Request, Response, NextFunction, RequestHandler } from 'express';

export function asyncHandler(
  fn: (
    req: Request,
    res: Response,
    next: NextFunction,
  ) => Promise<unknown>,
): RequestHandler {
  return (req: Request, res: Response, next: NextFunction) => {
    Promise.resolve(fn(req, res, next)).catch(next);
  };
}

/**
 * Method decorator for async route handlers
 * Automatically catches errors and passes them to Express error handler
 *
 * Works with methods that have (req, res) or (req, res, next) signatures
 *
 * @example
 * class NotificationController {
 *   @AsyncHandler
 *   async getNotifications(req: Request, res: Response) {
 *     // Your code here - errors automatically caught!
 *   }
 * }
 */
export function AsyncHandler(
  _target: object,
  _propertyKey: string,
  descriptor: PropertyDescriptor,
): PropertyDescriptor {
  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
  const originalMethod = descriptor.value;

  if (typeof originalMethod !== 'function') {
    return descriptor;
  }

  // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
  descriptor.value = function (
    this: unknown,
    req: Request,
    res: Response,
    next: NextFunction,
  ) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment, @typescript-eslint/no-unsafe-call, @typescript-eslint/no-unsafe-member-access
    const result = originalMethod.call(this, req, res, next);

    // If it's a Promise, catch errors
    if (result && typeof result === 'object' && 'catch' in result) {
      // eslint-disable-next-line @typescript-eslint/no-unsafe-return, @typescript-eslint/no-unsafe-call, @typescript-eslint/no-unsafe-member-access
      return result.catch(next);
    }

    // eslint-disable-next-line @typescript-eslint/no-unsafe-return
    return result;
  };

  return descriptor;
}
