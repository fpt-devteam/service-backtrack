import { PaymentStatusType } from '@/src/domain/constants/payment-status.constant.js';

export type PaymentHistory = {
  id: string;
  userId: string;
  providerInvoiceId: string;
  amount: number;
  currency: string;
  status: PaymentStatusType;
  paymentDate: Date;
  createdAt: Date;
  updatedAt: Date;
}
