import { SubscriptionPlanType } from '@/src/domain/constants/subscription-plan.constant.js';
import { SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';

export type Subscription = {
  id: string;
  userId: string;
  providerSubscriptionId: string;
  planType: SubscriptionPlanType;
  status: SubscriptionStatusType;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  cancelAtPeriodEnd: boolean;
  createdAt: Date;
  updatedAt: Date;
}
