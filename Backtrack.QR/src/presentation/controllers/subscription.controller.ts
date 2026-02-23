import { Request, Response } from 'express';
import { isSuccess } from '@/src/shared/core/result.js';
import { ok, fail, getHttpStatus } from '@/src/presentation/contracts/common/api-response.js';
import { createSubscription } from '@/src/infrastructure/container.js';
import { getHeader, HeaderNames } from '@/src/presentation/utils/http-headers.util.js';
import { ServerErrors } from '@/src/application/errors/server.error.js';
import { CreateSubscriptionRequest } from '@/src/presentation/contracts/subscriptions/requests/create-subscription.request.js';

export const createSubscriptionAsync = async (req: Request, res: Response): Promise<void> => {
  const userId = getHeader(req, HeaderNames.AuthId);
  if (!userId) {
    res.status(500).json(fail(ServerErrors.MissingUserIdHeader));
    return;
  }

  const { priceId } = req.body as CreateSubscriptionRequest;
  const result = await createSubscription({ userId, priceId });

  if (isSuccess(result))
    res.status(201).json(ok(result.value));
  else
    res.status(getHttpStatus(result.error)).json(fail(result.error));
};
