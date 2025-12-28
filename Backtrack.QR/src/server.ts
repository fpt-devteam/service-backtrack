import express from 'express';
import { correlationMiddleware } from './presentation/middlewares/correlation.middleware.js';
import { errorMiddleware } from './presentation/middlewares/error.middleware.js';
import { loggingMiddleware } from './presentation/middlewares/logging.middleware.js';
import qrCodeRoute from './presentation/routes/qr-code.route.js';

export const app = express();

app.use(express.json());
app.use(correlationMiddleware);
app.use(loggingMiddleware);

app.get('/health', (req, res) => {
    res.json({ status: 'healthy' });
});

app.use('/', qrCodeRoute);

app.use(errorMiddleware);