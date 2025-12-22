import express from 'express';
import { correlationMiddleware } from './middlewares/correlation.middleware.js';
import { errorMiddleware } from './middlewares/error.middleware.js';
import { loggingMiddleware } from './middlewares/logging.middleware.js';
import itemRoute from './routes/item.route.js';

export const app = express();

app.use(express.json());
app.use(correlationMiddleware);
app.use(loggingMiddleware);

app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

app.use('/items', itemRoute);

app.use(errorMiddleware);