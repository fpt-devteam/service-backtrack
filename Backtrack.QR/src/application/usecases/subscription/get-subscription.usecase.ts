import { failure, Result, success } from '@/src/shared/core/result.js';
import { SubscriptionRepository } from '@/src/application/repositories/subscription.repository.js';
import { SubscriptionErrors } from '@/src/application/errors/subscription.error.js';
import { Subscription } from '@/src/domain/entities/subscription.entity.js';

type Deps = {
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
};

export const getSubscriptionUseCase = (deps: Deps) => async (input: Input): Promise<Result<Subscription>> => {
  const subscription = await deps.subscriptionRepository.findLatestByUserId(input.userId);
  if (!subscription) return failure(SubscriptionErrors.NotFound);
  return success(subscription);
};
