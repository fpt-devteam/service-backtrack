import Stripe from 'stripe';
import { failure, Result, success } from '@/src/shared/core/result.js';
import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';
import { SubscriptionPlan, SubscriptionPlanType } from '@/src/domain/constants/subscription-plan.constant.js';
import { SubscriptionStatus, SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { ServerErrors } from '@/src/application/errors/server.error.js';
import logger from '@/src/shared/core/logger.js';

type Deps = {
  userRepository: UserRepository;
  subscriptionRepository: SubscriptionRepository;
};

const mapPlanType = (stripeSubscription: Stripe.Subscription): SubscriptionPlanType => {
  const interval = stripeSubscription.items.data[0].price.recurring?.interval;
  return interval === 'year' ? SubscriptionPlan.Yearly : SubscriptionPlan.Monthly;
};

const mapStatus = (stripeStatus: Stripe.Subscription.Status): SubscriptionStatusType | null => {
  switch (stripeStatus) {
    case 'active': return SubscriptionStatus.Active;
    case 'past_due': return SubscriptionStatus.PastDue;
    case 'canceled': return SubscriptionStatus.Canceled;
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

  const planType = mapPlanType(stripeSubscription);
  const currentPeriodStart = new Date(stripeSubscription.items.data[0].current_period_start * 1000); // Stripe returns timestamps in seconds, convert to milliseconds
  const currentPeriodEnd = new Date(stripeSubscription.items.data[0].current_period_end * 1000); // Stripe returns timestamps in seconds, convert to milliseconds
  const cancelAtPeriodEnd = stripeSubscription.cancel_at_period_end;

  const existing = await deps.subscriptionRepository.findByProviderSubscriptionId(stripeSubscription.id);

  if (existing) {
    const updated = await deps.subscriptionRepository.update(existing.id, {
      planType,
      status,
      currentPeriodStart,
      currentPeriodEnd,
      cancelAtPeriodEnd,
    });
    return success(updated!);
  }

  const created = await deps.subscriptionRepository.save({
    userId: user.id,
    providerSubscriptionId: stripeSubscription.id,
    planType,
    status,
    currentPeriodStart,
    currentPeriodEnd,
    cancelAtPeriodEnd,
  });
  return success(created);
};
