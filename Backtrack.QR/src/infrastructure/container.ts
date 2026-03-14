import { createUserRepository } from '@/src/infrastructure/database/repositories/user.repository.js';
import { getQrByUserIdUseCase } from '@/src/application/usecases/qr/get-qr-by-user-id.usecase.js';
import { updateQrNoteUseCase } from '@/src/application/usecases/qr/update-qr-note.usecase.js';
import { createQrRepository } from '@/src/infrastructure/database/repositories/qr.repository.js';
import { createPaymentHistoryRepository } from '@/src/infrastructure/database/repositories/payment-history.repository.js';
import { createSubscriptionRepository } from '@/src/infrastructure/database/repositories/subscription.repository.js';
import { createSubscriptionPlanRepository } from '@/src/infrastructure/database/repositories/subscription-plan.repository.js';
import { createSubscriptionUseCase } from '@/src/application/usecases/subscription/create-subscription.usecase.js';
import { getSubscriptionUseCase } from '@/src/application/usecases/subscription/get-subscription.usecase.js';
import { getSubscriptionPlansUseCase } from '@/src/application/usecases/subscription/get-subscription-plans.usecase.js';
import { seedSubscriptionPlansUseCase } from '@/src/application/usecases/subscription/seed-subscription-plans.usecase.js';
import { cancelSubscriptionUseCase } from '@/src/application/usecases/subscription/cancel-subscription.usecase.js';
import { handleSubscriptionUpsertUseCase } from '@/src/application/usecases/subscription/handle-subscription-upsert.usecase.js';
import { handleInvoicePaymentUseCase } from '@/src/application/usecases/subscription/handle-invoice-payment.usecase.js';

// Layer 1: repositories
const userRepository = createUserRepository();
const qrRepository = createQrRepository();
const subscriptionRepository = createSubscriptionRepository();
const subscriptionPlanRepository = createSubscriptionPlanRepository();
const paymentHistoryRepository = createPaymentHistoryRepository();

// Layer 2: use cases (inject repositories)
// QR use cases
export const getQrByUserId = getQrByUserIdUseCase({ qrRepository });
export const updateQrNote = updateQrNoteUseCase({ qrRepository });

// Subscription use cases
export const getSubscription = getSubscriptionUseCase({ subscriptionRepository });
export const getSubscriptionPlans = getSubscriptionPlansUseCase({ subscriptionPlanRepository });
export const seedSubscriptionPlans = seedSubscriptionPlansUseCase({ subscriptionPlanRepository });
export const createSubscription = createSubscriptionUseCase({ userRepository, subscriptionRepository });
export const cancelSubscription = cancelSubscriptionUseCase({ subscriptionRepository });
export const handleSubscriptionUpsert = handleSubscriptionUpsertUseCase({ userRepository, subscriptionRepository, subscriptionPlanRepository });
export const handleInvoicePayment = handleInvoicePaymentUseCase({ userRepository, paymentHistoryRepository });
