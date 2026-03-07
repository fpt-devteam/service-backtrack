import { Router } from 'express';
import asyncHandler from 'express-async-handler';

import * as SubscriptionController from '@/src/presentation/controllers/subscription.controller.js';
import { validateBody } from '@/src/presentation/middlewares/validation.middleware.js';
import { CreateSubscriptionRequestSchema } from '@/src/presentation/contracts/subscriptions/requests/create-subscription.request.js';

const router = Router();

router.get('/me', asyncHandler(SubscriptionController.getSubscriptionAsync));
router.post('/', validateBody(CreateSubscriptionRequestSchema), asyncHandler(SubscriptionController.createSubscriptionAsync));
router.delete('/me', asyncHandler(SubscriptionController.cancelSubscriptionAsync));

export default router;
