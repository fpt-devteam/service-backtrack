import { model, Schema, Types, Document } from 'mongoose';
import { SubscriptionPlan, SubscriptionPlanType } from '@/src/domain/constants/subscription-plan.constant.js';
import { SubscriptionStatus, SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';

export interface SubscriptionDocument extends Document {
  _id: Types.ObjectId;
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

const subscriptionSchema = new Schema<SubscriptionDocument>(
  {
    userId: { type: String, required: true, index: true },
    providerSubscriptionId: { type: String, required: true, index: true },
    planType: { type: String, enum: Object.values(SubscriptionPlan), required: true },
    status: { type: String, enum: Object.values(SubscriptionStatus), required: true },
    currentPeriodStart: { type: Date, required: true },
    currentPeriodEnd: { type: Date, required: true },
    cancelAtPeriodEnd: { type: Boolean, default: false },
  },
  { timestamps: true }
);

export const SubscriptionModel = model<SubscriptionDocument>('Subscription', subscriptionSchema);
