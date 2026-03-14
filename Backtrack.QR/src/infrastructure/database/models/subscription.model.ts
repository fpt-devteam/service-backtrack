import { model, Schema, Types, Document } from 'mongoose';
import { SubscriptionStatus, SubscriptionStatusType } from '@/src/domain/constants/subscription-status.constant.js';
import { SubscriptionPlanSnapshot } from '@/src/domain/entities/subscription-plan.entity.js';

export interface SubscriptionDocument extends Document {
  _id: Types.ObjectId;
  userId: string;
  planId: string;
  providerSubscriptionId: string;
  status: SubscriptionStatusType;
  subscriptionPlanSnapshot: SubscriptionPlanSnapshot;
  cancelAtPeriodEnd: boolean;
  currentPeriodStart: Date;
  currentPeriodEnd: Date;
  createdAt: Date;
  updatedAt: Date;
}

const subscriptionPlanSnapshotSchema = new Schema<SubscriptionPlanSnapshot>(
  {
    name: { type: String, required: true },
    price: { type: Number, required: true },
    currency: { type: String, required: true },
    features: { type: [String], default: [] },
  },
  { _id: false }
);

const subscriptionSchema = new Schema<SubscriptionDocument>(
  {
    userId: { type: String, required: true, index: true },
    planId: { type: String, required: true },
    providerSubscriptionId: { type: String, required: true, index: true },
    status: { type: String, enum: Object.values(SubscriptionStatus), required: true },
    subscriptionPlanSnapshot: { type: subscriptionPlanSnapshotSchema, required: true },
    currentPeriodStart: { type: Date, required: true },
    currentPeriodEnd: { type: Date, required: true },
    cancelAtPeriodEnd: { type: Boolean, required: true },
  },
  { timestamps: true }
);

export const SubscriptionModel = model<SubscriptionDocument>('Subscription', subscriptionSchema);
