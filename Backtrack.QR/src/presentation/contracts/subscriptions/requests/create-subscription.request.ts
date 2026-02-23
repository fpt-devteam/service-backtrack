import { z } from 'zod';

export type CreateSubscriptionRequest = {
  priceId: string;
};

export const CreateSubscriptionRequestSchema = z.object({
  priceId: z.string().trim().min(1, 'priceId is required'),
}).strict();
