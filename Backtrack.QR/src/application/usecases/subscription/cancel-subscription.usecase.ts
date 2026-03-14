import { failure, Result, success } from '@/src/shared/core/result.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { SubscriptionErrors } from '@/src/application/errors/subscription.error.js';
import { ONGOING_SUBSCRIPTION_STATUSES } from '@/src/domain/constants/subscription-status.constant.js';
import { stripe } from '@/src/infrastructure/configs/stripe.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';

type Deps = {
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
};

export const cancelSubscriptionUseCase = (deps: Deps) => async (input: Input): Promise<Result<Subscription>> => {
  const subscription = await deps.subscriptionRepository.findLatestByUserId(input.userId);

  if (!subscription || !(ONGOING_SUBSCRIPTION_STATUSES as readonly string[]).includes(subscription.status)) {
    return failure(SubscriptionErrors.NoActiveSubscription);
  }

  await stripe.subscriptions.update(subscription.providerSubscriptionId, { cancel_at_period_end: true });

  const updated = await deps.subscriptionRepository.update(subscription.id, { cancelAtPeriodEnd: true });
  return success(updated!);
};
