import type { Error } from '@/src/shared/core/error.js';

export const SubscriptionErrors = {
  AlreadySubscribed: {
    kind: 'Validation',
    code: 'SubscriptionAlreadySubscribed',
    message: 'User already has an active subscription. Cancel the current subscription before creating a new one.',
  } as Error,
  NotFound: {
    kind: 'NotFound',
    code: 'SubscriptionNotFound',
    message: 'No subscription found for this user.',
  } as Error,
} as const satisfies Record<string, Error>;
