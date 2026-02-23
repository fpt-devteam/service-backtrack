import { PaymentHistory } from '@/src/domain/entities/payment-history.entity.js';

export type PaymentHistoryRepository = {
  findById: (id: string) => Promise<PaymentHistory | null>;
  findByUserId: (userId: string) => Promise<PaymentHistory[]>;
  findByProviderInvoiceId: (providerInvoiceId: string) => Promise<PaymentHistory | null>;
  save: (paymentHistory: Omit<PaymentHistory, 'id' | 'createdAt' | 'updatedAt'>) => Promise<PaymentHistory>;
};
