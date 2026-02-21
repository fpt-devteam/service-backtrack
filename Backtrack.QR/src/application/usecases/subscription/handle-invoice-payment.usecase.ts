import Stripe from 'stripe';
import { failure, Result, success } from '@/src/shared/core/result.js';
import { UserRepository } from '@/src/application/repositories/user.repository.js';
import { PaymentHistoryRepository } from '@/src/application/repositories/payment-history.repository.js';
import { PaymentHistory } from '@/src/domain/entities/payment-history.entity.js';
import { PaymentStatusType } from '@/src/domain/constants/payment-status.constant.js';
import { ServerErrors } from '@/src/application/errors/server.error.js';

type Deps = {
  userRepository: UserRepository;
  paymentHistoryRepository: PaymentHistoryRepository;
};

export const handleInvoicePaymentUseCase = (deps: Deps) => async (invoice: Stripe.Invoice, status: PaymentStatusType): Promise<Result<PaymentHistory>> => {
  const customerId = typeof invoice.customer === 'string'
    ? invoice.customer
    : invoice.customer!.id;

  const user = await deps.userRepository.findByProviderCustomerId(customerId);
  if (!user) return failure(ServerErrors.ProviderCustomerIdNotFound);

  const paymentHistory = await deps.paymentHistoryRepository.save({
    userId: user.id,
    providerInvoiceId: invoice.id,
    amount: invoice.amount_paid / 100, // Stripe amounts are in cents, convert to dollars
    currency: invoice.currency,
    status,
    paymentDate: new Date(invoice.created * 1000), // Stripe returns timestamps in seconds, convert to milliseconds
  });

  return success(paymentHistory);
};
