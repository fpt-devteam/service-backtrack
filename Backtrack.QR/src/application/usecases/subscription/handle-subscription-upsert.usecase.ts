import Stripe from 'stripe';
import { failure, Result, success } from '@/src/shared/core/result.js';
import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { SubscriptionPlanRepository } from '@/src/application/repositories/subscription-plan.repository.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';
import { SubscriptionStatus, SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { ServerErrors } from '@/src/application/errors/server.error.js';
import logger from '@/src/shared/core/logger.js';
import { StripeSubscriptionStatus } from '@/src/application/utils/stripe.util.js';

type Deps = {
  userRepository: UserRepository;
  subscriptionRepository: SubscriptionRepository;
  subscriptionPlanRepository: SubscriptionPlanRepository;
};

const mapStatus = (stripeStatus: Stripe.Subscription.Status): SubscriptionStatusType | null => {
  switch (stripeStatus) {
    case StripeSubscriptionStatus.ACTIVE: return SubscriptionStatus.Active;
    case StripeSubscriptionStatus.PAST_DUE: return SubscriptionStatus.PastDue;
    case StripeSubscriptionStatus.CANCELED: return SubscriptionStatus.Canceled;
    case StripeSubscriptionStatus.UNPAID: return SubscriptionStatus.Unpaid;
    case StripeSubscriptionStatus.INCOMPLETE: return SubscriptionStatus.Incomplete;
    case StripeSubscriptionStatus.INCOMPLETE_EXPIRED: return SubscriptionStatus.IncompleteExpired;
    default: return null;
  }
};

export const handleSubscriptionUpsertUseCase = (deps: Deps) => async (stripeSubscription: Stripe.Subscription): Promise<Result<Subscription | null>> => {
  const status = mapStatus(stripeSubscription.status);
  if (!status) {
    logger.info('Received subscription with no need handle status', { subscriptionId: stripeSubscription.id, status: stripeSubscription.status });
    return success(null);
  }

  const customerId = typeof stripeSubscription.customer === 'string'
    ? stripeSubscription.customer
    : stripeSubscription.customer.id;

  const user = await deps.userRepository.findByProviderCustomerId(customerId);
  if (!user) return failure(ServerErrors.ProviderCustomerIdNotFound);

  const priceId = stripeSubscription.items.data[0].price.id;
  const plan = await deps.subscriptionPlanRepository.findByProviderPriceId(priceId);
  if (!plan) return failure(ServerErrors.SubscriptionPlanNotFound);

  const periodStart = new Date(stripeSubscription.items.data[0].current_period_start * 1000);
  const periodEnd = new Date(stripeSubscription.items.data[0].current_period_end * 1000);

  const subscriptionPlanSnapshot = {
    name: plan.name,
    price: plan.price,
    currency: plan.currency,
    features: plan.features,
  };

  const existing = await deps.subscriptionRepository.findByProviderSubscriptionId(stripeSubscription.id);

  if (existing) {
    const updated = await deps.subscriptionRepository.update(existing.id, {
      planId: plan.id,
      status,
      subscriptionPlanSnapshot,
      currentPeriodStart: periodStart,
      currentPeriodEnd: periodEnd,
      cancelAtPeriodEnd: stripeSubscription.cancel_at_period_end,
    });
    return success(updated!);
  }

  const created = await deps.subscriptionRepository.save({
    userId: user.id,
    planId: plan.id,
    providerSubscriptionId: stripeSubscription.id,
    status,
    subscriptionPlanSnapshot,
    currentPeriodStart: periodStart,
    currentPeriodEnd: periodEnd,
    cancelAtPeriodEnd: stripeSubscription.cancel_at_period_end,
  });
  return success(created);
};
