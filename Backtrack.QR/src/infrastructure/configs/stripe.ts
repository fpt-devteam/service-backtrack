import Stripe from 'stripe';
import { env } from '@/src/infrastructure/configs/env.js';
export const stripe = new Stripe(env.STRIPE_SECRET_KEY);