import { SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { SubscriptionPlanSnapshot } from '@/src/domain/entities/subscription-plan.entity.js';

export type Subscription = {
  id: string;
  userId: string;
  planId: string;
  providerSubscriptionId: string;
  status: SubscriptionStatusType;
  subscriptionPlanSnapshot: SubscriptionPlanSnapshot;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  cancelAtPeriodEnd: boolean;
  createdAt: Date;
  updatedAt: Date;
};
