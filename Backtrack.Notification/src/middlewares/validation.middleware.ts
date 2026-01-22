import HTTP_STATUS_CODES from '@src/common/constants/HTTP_STATUS_CODES'
import { AppError } from '@src/common/errors/app-error'
import type { NextFunction, Request, Response } from 'express'
import { z } from 'zod'

function zodToAppErrorFirst(err: z.ZodError): AppError {
  const first = err.issues[0]
  const message = first?.message

  const details = {
    issue: first,
    path: first?.path?.length ? first.path.join('.') : '_root',
    type: first?.code,
  }

  return new AppError(
    'VALIDATION_ERROR',
    message,
    HTTP_STATUS_CODES.BadRequest,
    details,
  )
}

export function validateBody<T extends z.ZodTypeAny>(schema: T) {
  return (req: Request, _res: Response, next: NextFunction) => {
    const result = schema.safeParse(req.body)

    if (!result.success) {
      return next(zodToAppErrorFirst(result.error))
    }

    req.body = result.data
    next()
  }
}
