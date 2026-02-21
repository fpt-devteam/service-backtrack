export const PaymentStatus = {
  Succeeded: 'Succeeded',
  Failed: 'Failed',
  Pending: 'Pending',
} as const;

export type PaymentStatusType = typeof PaymentStatus[keyof typeof PaymentStatus];

const PAYMENT_STATUS_VALUES = Object.values(PaymentStatus) as readonly PaymentStatusType[];

export function parsePaymentStatus(input: unknown): PaymentStatusType | null {
  if (typeof input !== 'string') return null;
  return (PAYMENT_STATUS_VALUES as readonly string[]).includes(input) ? (input as PaymentStatusType) : null;
}
