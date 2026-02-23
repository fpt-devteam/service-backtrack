import { failure, Result, success } from "@/src/shared/core/result.js";
import { UserRepository } from "@/src/application/repositories/user.repository.js";
import { SubscriptionRepository } from "@/src/application/repositories/subscription.repository.js";
import { UserErrors } from "@/src/application/errors/user.error.js";
import { SubscriptionErrors } from "@/src/application/errors/subscription.error.js";
import { SubscriptionStatus } from "@/src/domain/constants/subscription-status.constant.js";
import { stripe } from "@/src/infrastructure/configs/stripe.js";
import { Stripe } from "stripe";

type Deps = {
  userRepository: UserRepository;
  subscriptionRepository: SubscriptionRepository;
};

type Input = {
  userId: string;
  priceId: string;
};

export const createSubscriptionUseCase = (deps: Deps) => async (input: Input): Promise<Result<{ clientSecret: string }>> => {
  const user = await deps.userRepository.findById(input.userId);
  if (!user) return failure(UserErrors.NotFound);
  if (!user.email) return failure(UserErrors.AnonymousForbidden);

  const latest = await deps.subscriptionRepository.findLatestByUserId(input.userId);
  if (latest && latest.status !== SubscriptionStatus.Canceled) {
    return failure(SubscriptionErrors.AlreadyActive);
  }

  let providerCustomerId = user.providerCustomerId;
  if (!providerCustomerId) {
    const customer = await stripe.customers.create({ email: user.email });
    providerCustomerId = customer.id;
    await deps.userRepository.update(user.id, { providerCustomerId });
  }

  const subscription = await stripe.subscriptions.create({
    customer: providerCustomerId,
    items: [{ price: input.priceId }],
    payment_behavior: 'default_incomplete',
    expand: ['latest_invoice.confirmation_secret'],
  });

  const invoice = subscription.latest_invoice as Stripe.Invoice;
  return success({ clientSecret: invoice.confirmation_secret!.client_secret });
};