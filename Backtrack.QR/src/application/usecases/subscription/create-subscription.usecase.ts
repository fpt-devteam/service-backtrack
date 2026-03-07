import { failure, Result, success } from "@/src/shared/core/result.js";
import { UserRepository } from "@/src/application/repositories/user.repository.js";
import { SubscriptionRepository } from "@/src/application/repositories/subscription.repository.js";
import { UserErrors } from "@/src/application/errors/user.error.js";
import { SubscriptionErrors } from "@/src/application/errors/subscription.error.js";
import { stripe } from "@/src/infrastructure/configs/stripe.js";
import { Stripe } from "stripe";
import { Subscription } from "@/src/domain/entities/subscription.entity.js";
import { ONGOING_SUBSCRIPTION_STATUSES, PENDING_PAYMENT_STATUSES, TERMINATED_SUBSCRIPTION_STATUSES } from "@/src/application/utils/stripe.util.js";
import { ServerErrors } from "@/src/application/errors/server.error.js";
import logger from "@/src/shared/core/logger.js";

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
  const providerCustomerId = user.providerCustomerId || await ensureProviderCustomerIdExists(deps, user.id, user.email);

  const latestSub = await deps.subscriptionRepository.findLatestByUserId(input.userId);

  if (!latestSub) {
    return await createNewSubscription(providerCustomerId, input.priceId);
  }

  const providerSub = await stripe.subscriptions.retrieve(latestSub.providerSubscriptionId, {
    expand: ['latest_invoice.confirmation_secret']
  });

  if (TERMINATED_SUBSCRIPTION_STATUSES.includes(providerSub.status)) {
    return await createNewSubscription(providerCustomerId, input.priceId);
  }

  if (ONGOING_SUBSCRIPTION_STATUSES.includes(providerSub.status)) {
    return failure(SubscriptionErrors.AlreadySubscribed);
  }
  if (PENDING_PAYMENT_STATUSES.includes(providerSub.status)) {
    return await reuseClientSecret(deps, latestSub, input.priceId);
  }

  logger.error('Unexpected subscription status', { userId: input.userId, priceId: input.priceId, status: providerSub.status });

  return failure(ServerErrors.UnexpectedError);
};

const reuseClientSecret = async (deps: Deps, latestSub: Subscription, priceId: string) => {
  const stripeSub = await stripe.subscriptions.retrieve(latestSub.providerSubscriptionId, {
    expand: ['latest_invoice.confirmation_secret']
  });
  const currentPriceId = stripeSub.items.data[0].price.id;

  // same priceId, just return existing client secret
  if (currentPriceId === priceId) {
    const invoice = stripeSub.latest_invoice as Stripe.Invoice;
    return success({ clientSecret: invoice.confirmation_secret!.client_secret });
  }

  // different priceId, update subscription to new price and return new client secret
  else {
    const updatedSub = await stripe.subscriptions.update(stripeSub.id, {
      items: [{
        id: stripeSub.items.data[0].id,
        price: priceId,
      }],
      expand: ['latest_invoice.confirmation_secret']
    });

    const invoice = updatedSub.latest_invoice as Stripe.Invoice;
    return success({ clientSecret: invoice.confirmation_secret!.client_secret });
  }
};

const ensureProviderCustomerIdExists = async (deps: Deps, userId: string, email: string): Promise<string> => {
  const customer = await stripe.customers.create({ email });

  const providerCustomerId = await deps.userRepository.setProviderCustomerIdIfNull(userId, customer.id);

  if (providerCustomerId !== customer.id) {
    await stripe.customers.del(customer.id);
  }
  return providerCustomerId;
};

const createNewSubscription = async (providerCustomerId: string, priceId: string) => {
  const subscription = await stripe.subscriptions.create({
    customer: providerCustomerId,
    items: [{ price: priceId }],
    payment_behavior: 'default_incomplete',
    expand: ['latest_invoice.confirmation_secret'],
  });

  const invoice = subscription.latest_invoice as Stripe.Invoice;
  return success({ clientSecret: invoice.confirmation_secret!.client_secret });
};