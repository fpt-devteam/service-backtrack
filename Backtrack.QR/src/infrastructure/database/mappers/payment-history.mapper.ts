import { PaymentHistory } from '@/src/domain/entities/payment-history.entity.js';
import { PaymentHistoryDocument } from '@/src/infrastructure/database/models/payment-history.model.js';

export const paymentHistoryToDomain = (doc: PaymentHistoryDocument): PaymentHistory => ({
  id: doc._id.toString(),
  userId: doc.userId,
  providerInvoiceId: doc.providerInvoiceId,
  amount: doc.amount,
  currency: doc.currency,
  status: doc.status,
  paymentDate: doc.paymentDate,
  createdAt: doc.createdAt,
  updatedAt: doc.updatedAt,
});
