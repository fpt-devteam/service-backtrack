import { Result, success } from '@/src/shared/core/result.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { ONGOING_SUBSCRIPTION_STATUSES, OngoingSubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { SubscriptionPlanSnapshot } from '@/src/domain/entities/subscription-plan.entity.js';

type Deps = {
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
};

export type SubscriptionInfo = {
  id: string;
  userId: string;
  planId: string;
  subscriptionPlanSnapshot: SubscriptionPlanSnapshot;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  cancelAtPeriodEnd: boolean;
  status: OngoingSubscriptionStatusType;
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
    planId: subscription.planId,
    subscriptionPlanSnapshot: subscription.subscriptionPlanSnapshot,
    currentPeriodStart: subscription.currentPeriodStart,
    currentPeriodEnd: subscription.currentPeriodEnd,
    cancelAtPeriodEnd: subscription.cancelAtPeriodEnd,
    status: subscription.status as OngoingSubscriptionStatusType,
  });
};
