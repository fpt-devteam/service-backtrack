import { model, Schema, Types, Document } from 'mongoose';

export interface SubscriptionPlanDocument extends Document {
  _id: Types.ObjectId;
  name: string;
  price: number;
  currency: string;
  providerPriceId: string;
  features: string[];
  createdAt: Date;
  updatedAt: Date;
}

const subscriptionPlanSchema = new Schema<SubscriptionPlanDocument>(
  {
    name: { type: String, required: true },
    price: { type: Number, required: true },
    currency: { type: String, required: true },
    providerPriceId: { type: String, required: true, unique: true, index: true },
    features: { type: [String], default: [] },
  },
  { timestamps: true }
);

export const SubscriptionPlanModel = model<SubscriptionPlanDocument>('SubscriptionPlan', subscriptionPlanSchema);
