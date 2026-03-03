import express, { Express } from 'express';
import cors from 'cors';
import helmet from 'helmet';
import compression from 'compression';
import rateLimit from 'express-rate-limit';
import path from 'path';

import morganMiddleware from '@/middlewares/morgan.middleware';
import errorHandler from '@/middlewares/error-handler';
import router from '@/routes/index';

const app: Express = express();

// Trust proxy - needed when behind API Gateway/reverse proxy
app.set('trust proxy', true);

// Logging
app.use(morganMiddleware);

// Security & Performance
app.use(helmet());
app.use(compression());
app.use(
	rateLimit({
		windowMs: 15 * 60 * 1000,
		max: 100,
		standardHeaders: true,
		validate: { trustProxy: false }, // Disable validation since we're behind API Gateway
	}),
);

// CORS
app.use(
	cors({
		origin: process.env.CLIENT_URL || 'http://localhost:3000',
		credentials: true,
	}),
);

// Body Parsing
app.use(express.json({ limit: '10mb' }));
app.use(express.urlencoded({ extended: true }));

// Static files (dev test UI)
if (process.env.NODE_ENV !== 'production') {
	app.use(express.static(path.join(__dirname, '../public')));
}

// Routes
app.use(router);

// Error Handling
app.use(errorHandler);

export default app;
