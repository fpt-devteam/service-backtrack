import morgan from 'morgan';
import helmet from 'helmet';
import cors from 'cors';
import express, { Request, Response, Express } from 'express';
import messageRoute from '@src/routes/messageRoute';
import conversationRoute from '@src/routes/conversationRoute';
import { errorHandler } from '@src/middlewares/errorHandler';
import { correlationIdMiddleware } from '@src/middlewares/correlationId';
import ENV from '@src/common/constants/ENV';

const app: Express = express();

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(helmet());

if (ENV.NodeEnv === 'development') {
  app.use(morgan('dev'));
}

// Add correlationId to every request
app.use(correlationIdMiddleware);

// Health check endpoint
app.get('/health', (_: Request, res: Response) => {
  res.json({ status: 'healthy' });
});

// API Routes
app.use('/conversations', conversationRoute);
app.use('/messages', messageRoute);

// 404 handler
app.use((req: Request, res: Response) => {
  res.status(404).json({
    success: false,
    error: {
      code: 'NotFound',
      message: `Cannot ${req.method} ${req.path}`,
    },
  });
});

// Error handler must be last
app.use(errorHandler);

export default app;
