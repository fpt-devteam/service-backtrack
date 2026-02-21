import type { Error } from '@/src/shared/core/error.js';

export const SubscriptionErrors = {
  AlreadyActive: {
    kind: 'Validation',
    code: 'SubscriptionAlreadyActive',
    message: 'User already has an active subscription.',
  } as Error,
} as const satisfies Record<string, Error>;
