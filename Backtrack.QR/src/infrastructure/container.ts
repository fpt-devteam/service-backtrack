import { createUserRepository } from '@/src/infrastructure/database/repositories/user.repository.js';
import { getQrByUserIdUseCase } from '@/src/application/usecases/qr/get-qr-by-user-id.usecase.js';
import { createQrRepository } from '@/src/infrastructure/database/repositories/qr.repository.js';
import { createPaymentHistoryRepository } from '@/src/infrastructure/database/repositories/payment-history.repository.js';
import { createSubscriptionRepository } from '@/src/infrastructure/database/repositories/subscription.repository.js';
import { createSubscriptionUseCase } from '@/src/application/usecases/subscription/create-subscription.usecase.js';
import { handleSubscriptionUpsertUseCase } from '@/src/application/usecases/subscription/handle-subscription-upsert.usecase.js';
import { handleInvoicePaymentUseCase } from '@/src/application/usecases/subscription/handle-invoice-payment.usecase.js';

// Layer 1: repositories
const userRepository = createUserRepository();
const qrRepository = createQrRepository();
const subscriptionRepository = createSubscriptionRepository();
const paymentHistoryRepository = createPaymentHistoryRepository();

// Layer 2: use cases (inject repositories)
// QR use cases
export const getQrByUserId = getQrByUserIdUseCase({ qrRepository });

// Subscription use cases
export const createSubscription = createSubscriptionUseCase({ userRepository, subscriptionRepository });
export const handleSubscriptionUpsert = handleSubscriptionUpsertUseCase({ userRepository, subscriptionRepository });
export const handleInvoicePayment = handleInvoicePaymentUseCase({ userRepository, paymentHistoryRepository });