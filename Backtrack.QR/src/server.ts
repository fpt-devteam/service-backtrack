import express from 'express';
import { errorMiddleware } from '@/src/presentation/middlewares/error.middleware.js';
import { loggingMiddleware } from '@/src/presentation/middlewares/logging.middleware.js';
import { correlationMiddleware } from '@/src/presentation/middlewares/correlation.middleware.js';
import qrRoute from '@/src/presentation/routes/qr.route.js';
import subscriptionRoute from '@/src/presentation/routes/subscription.route.js';
import webhookRoute from '@/src/presentation/routes/webhook.route.js';

export const app = express();

// Webhook must use raw body before express.json() parses it
app.use('/webhooks', express.raw({ type: 'application/json' }), webhookRoute);

app.use(express.json());
app.use(correlationMiddleware);
app.use(loggingMiddleware);

app.get('/health', (req, res) => {
  res.json({ status: 'healthy' });
});
app.use('/', qrRoute);
app.use('/subscriptions', subscriptionRoute);
app.use(errorMiddleware);