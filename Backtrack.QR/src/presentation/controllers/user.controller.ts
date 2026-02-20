import { Request, Response } from 'express';
import { isFailure } from '@/src/shared/core/result.js';
import { ok, fail, getHttpStatus } from '@/src/presentation/contracts/common/api-response.js';
import { getUserById, createUser } from '@/src/infrastructure/container.js';
import * as logger from '@/src/shared/core/logger.js';
import { CreateUserRequest } from '@/src/presentation/contracts/users/requests/create-user.request.js';

export const getUserByIdAsync = async (req: Request, res: Response): Promise<void> => {
  const result = await getUserById(req.params.id);

  if (isFailure(result)) {
    logger.warn('Failed to get user by ID', {
      error: result.error,
      correlationId: req.correlationId
    });
    res.status(getHttpStatus(result.error)).json(fail(result.error.kind, result.error.message));
    return;
  }

  res.json(ok(result.value));
};

export const createUserAsync = async (req: Request, res: Response): Promise<void> => {
  const body: CreateUserRequest = req.body;
  const result = await createUser({
    email: body.email,
    displayName: body.displayName,
    avatarUrl: body.avatarUrl,
    globalRole: body.globalRole,
    syncedAt: new Date(),
  });

  if (isFailure(result)) {
    logger.warn('Failed to create user', {
      error: result.error,
      correlationId: req.correlationId
    });
    res.status(getHttpStatus(result.error)).json(fail(result.error.kind, result.error.message));
    return;
  }

  res.status(201).json(ok(result.value));
};