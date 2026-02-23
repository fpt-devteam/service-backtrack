import { Request, Response } from 'express';
import Stripe from 'stripe';
import { stripe } from '@/src/infrastructure/configs/stripe.js';
import { env } from '@/src/infrastructure/configs/env.js';
import { fail, getHttpStatus, ok } from '@/src/presentation/contracts/common/api-response.js';
import { createError } from '@/src/shared/core/error.js';
import { handleSubscriptionUpsert, handleInvoicePayment } from '@/src/infrastructure/container.js';
import { isSuccess } from '@/src/shared/core/result.js';
import { PaymentStatus } from '@/src/domain/constants/payment-status.constant.js';
import logger from '@/src/shared/core/logger.js';

export const handleStripeWebhookAsync = async (req: Request, res: Response): Promise<void> => {
  const sig = req.headers['stripe-signature'] as string;

  let event: Stripe.Event;
  try {
    event = stripe.webhooks.constructEvent(req.body, sig, env.STRIPE_WEBHOOK_SECRET);
  } catch {
    const err = createError('Validation', 'InvalidWebhookSignature', 'Webhook signature verification failed');
    res.status(400).json(fail(err));
    return;
  }

  logger.debug('Received Stripe webhook event', { eventType: event.type, eventId: event.id });

  switch (event.type) {
    case 'customer.subscription.deleted':
    case 'customer.subscription.created':
    case 'customer.subscription.updated': {
      const result = await handleSubscriptionUpsert(event.data.object);
      if (!isSuccess(result)) {
        logger.error('Failed to upsert subscription', { error: result.error, subscriptionId: event.data.object.id });
        res.status(getHttpStatus(result.error)).json(fail(result.error));
        return;
      }
      break;
    }
    case 'invoice.payment_succeeded': {
      const result = await handleInvoicePayment(event.data.object, PaymentStatus.Succeeded);
      if (!isSuccess(result)) {
        logger.error('Failed to handle invoice payment', { error: result.error, invoiceId: event.data.object.id });
        res.status(getHttpStatus(result.error)).json(fail(result.error));
        return;
      }
      break;
    }
    case 'invoice.payment_failed': {
      const result = await handleInvoicePayment(event.data.object, PaymentStatus.Failed);
      if (!isSuccess(result)) {
        logger.error('Failed to handle invoice payment failure', { error: result.error, invoiceId: event.data.object.id });
        res.status(getHttpStatus(result.error)).json(fail(result.error));
        return;
      }
      break;
    }
  }

  logger.debug('Successfully processed Stripe webhook event', { eventType: event.type, eventId: event.id });

  res.json(ok({ received: true }));
};
