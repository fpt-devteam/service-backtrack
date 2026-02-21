import { Router } from 'express';
import asyncHandler from 'express-async-handler';
import * as WebhookController from '@/src/presentation/controllers/webhook.controller.js';

const router = Router();

router.post('/stripe', asyncHandler(WebhookController.handleStripeWebhookAsync));

export default router;
