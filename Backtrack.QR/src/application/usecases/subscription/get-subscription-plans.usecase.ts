import { Result, success } from '@/src/shared/core/result.js';
import { SubscriptionPlanRepository } from '@/src/application/repositories/subscription-plan.repository.js';
import { SubscriptionPlan } from '@/src/domain/entities/subscription-plan.entity.js';

type Deps = {
  subscriptionPlanRepository: SubscriptionPlanRepository;
};

export const getSubscriptionPlansUseCase = (deps: Deps) => async (): Promise<Result<SubscriptionPlan[]>> => {
  const plans = await deps.subscriptionPlanRepository.findAll();
  return success(plans);
};
