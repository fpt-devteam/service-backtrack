import express from 'express';
import { errorMiddleware } from '@/src/presentation/middlewares/error.middleware.js';
import { loggingMiddleware } from '@/src/presentation/middlewares/logging.middleware.js';
import userRoute from '@/src/presentation/routes/user.route.js';
import { correlationMiddleware } from '@/src/presentation/middlewares/correlation.middleware.js';

export const app = express();

app.use(express.json());
app.use(correlationMiddleware);
app.use(loggingMiddleware);

app.get('/health', (req, res) => {
  res.json({ status: 'healthy' });
});
app.use('/users', userRoute);
app.use(errorMiddleware);