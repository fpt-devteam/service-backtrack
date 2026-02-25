import { Result, success } from '@/src/shared/core/result.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';
import { ONGOING_SUBSCRIPTION_STATUSES, OngoingSubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';

type Deps = {
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
};

type OngoingSubscriptionResult = Omit<Subscription, 'status'> & {
  subscriptionStatus: 'ONGOING';
  statusDetail: OngoingSubscriptionStatusType;
};

type NoCurrentSubscriptionResult = {
  subscriptionStatus: 'NO_CURRENT_SUBSCRIPTION';
};

export type GetSubscriptionResult = OngoingSubscriptionResult | NoCurrentSubscriptionResult;

export const getSubscriptionUseCase = (deps: Deps) => async (input: Input): Promise<Result<GetSubscriptionResult>> => {
  const subscription = await deps.subscriptionRepository.findLatestByUserId(input.userId);

  if (!subscription || !(ONGOING_SUBSCRIPTION_STATUSES as readonly string[]).includes(subscription.status)) {
    return success({ subscriptionStatus: 'NO_CURRENT_SUBSCRIPTION' });
  }

  const { status, ...rest } = subscription;
  return success({
    ...rest,
    subscriptionStatus: 'ONGOING',
    statusDetail: status as OngoingSubscriptionStatusType,
  });
};
