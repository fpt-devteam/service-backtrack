import { PaymentHistoryRepository } from '@/src/application/repositories/payment-history.repository.js';
import { PaymentHistoryModel } from '@/src/infrastructure/database/models/payment-history.model.js';
import { paymentHistoryToDomain } from '@/src/infrastructure/database/mappers/payment-history.mapper.js';
import { PaymentHistory } from '@/src/domain/entities/payment-history.entity.js';

export const createPaymentHistoryRepository = (): PaymentHistoryRepository => ({
  findById: async (id: string) => {
    const doc = await PaymentHistoryModel.findById(id);
    return doc ? paymentHistoryToDomain(doc) : null;
  },

  findByUserId: async (userId: string) => {
    const docs = await PaymentHistoryModel.find({ userId });
    return docs.map(paymentHistoryToDomain);
  },

  findByProviderInvoiceId: async (providerInvoiceId: string) => {
    const doc = await PaymentHistoryModel.findOne({ providerInvoiceId });
    return doc ? paymentHistoryToDomain(doc) : null;
  },

  save: async (paymentHistory: Omit<PaymentHistory, 'id' | 'createdAt' | 'updatedAt'>) => {
    const doc = await PaymentHistoryModel.create(paymentHistory);
    return paymentHistoryToDomain(doc);
  },
});
