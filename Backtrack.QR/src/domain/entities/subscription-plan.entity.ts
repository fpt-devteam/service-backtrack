export type SubscriptionPlan = {
  id: string;
  name: string;
  price: number;
  currency: string;
  providerPriceId: string;
  features: string[];
  createdAt: Date;
  updatedAt: Date;
};

export type SubscriptionPlanSnapshot = {
  name: string;
  price: number;
  currency: string;
  features: string[];
};
