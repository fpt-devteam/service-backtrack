import { Result, success } from '@/src/shared/core/result.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { ONGOING_SUBSCRIPTION_STATUSES, OngoingSubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { SubscriptionPlanType } from '@/src/domain/constants/subscription-plan.constant.js';

type Deps = {
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
};

export type SubscriptionInfo = {
  id: string;
  userId: string;
  planType: SubscriptionPlanType;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  status: OngoingSubscriptionStatusType;
  cancelAtPeriodEnd: boolean;
};

export type GetSubscriptionResult = SubscriptionInfo | null;

export const getSubscriptionUseCase = (deps: Deps) => async (input: Input): Promise<Result<GetSubscriptionResult>> => {
  const subscription = await deps.subscriptionRepository.findLatestByUserId(input.userId);

  if (!subscription || !(ONGOING_SUBSCRIPTION_STATUSES as readonly string[]).includes(subscription.status)) {
    return success(null);
  }

  return success({
    id: subscription.id,
    userId: subscription.userId,
    planType: subscription.planType,
    currentPeriodStart: subscription.currentPeriodStart,
    currentPeriodEnd: subscription.currentPeriodEnd,
    status: subscription.status as OngoingSubscriptionStatusType,
    cancelAtPeriodEnd: subscription.cancelAtPeriodEnd,
  });
};
