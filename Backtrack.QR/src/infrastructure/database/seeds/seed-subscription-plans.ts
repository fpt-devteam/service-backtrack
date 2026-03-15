import dotenv from 'dotenv';
import path from 'node:path';

dotenv.config({ path: path.resolve(process.cwd(), '../env/backtrack-qr-api.docker.env') });

// Dynamic imports so env.ts validates AFTER dotenv has populated process.env
const { default: mongoose } = await import('mongoose');
const { SubscriptionPlanModel } = await import('@/src/infrastructure/database/models/subscription-plan.model.js');
const { env } = await import('@/src/infrastructure/configs/env.js');

const DB_NAME = 'backtrack-qr-db';

const plans = [
  {
    name: 'Monthly',
    price: 4.99,
    currency: 'usd',
    providerPriceId: 'price_1T39LrQqedaIws156XykqIYL',
    features: [
      'Print unlimited QR codes',
      'Setup your Backtrack profile',
      'Custom QR design & branding',
    ],
  },
  {
    name: 'Yearly',
    price: 49.99,
    currency: 'usd',
    providerPriceId: 'price_1T3B5zQqedaIws15A0XEX6HR',
    features: [
      'Print unlimited QR codes',
      'Setup your Backtrack profile',
      'Custom QR design & branding',
      '2 months free',
    ],
  },
];

await mongoose.connect(env.DATABASE_URI, { dbName: DB_NAME });
console.log('Connected to MongoDB');

for (const plan of plans) {
  const existing = await SubscriptionPlanModel.findOne({ name: plan.name });
  if (existing) {
    console.log(`[SKIP] Plan "${plan.name}" already exists`);
    continue;
  }

  await SubscriptionPlanModel.create(plan);
  console.log(`[OK]   Plan "${plan.name}" seeded — price: ${plan.currency.toUpperCase()} ${plan.price}`);
}

await mongoose.disconnect();
console.log('Done');
