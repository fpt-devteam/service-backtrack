import morgan from 'morgan';
import path from 'path';
import helmet from 'helmet';
import express, { Request, Response, Express } from 'express';
import messageRoute from '@src/routes/messageRoute';
import conversationRoute from '@src/routes/conversationRoute';
import { errorHandler } from '@src/middlewares/errorHandler';
import { correlationIdMiddleware } from '@src/middlewares/correlationId';
import ENV from '@src/common/constants/ENV';

const app: Express = express();

// Middleware
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(helmet());

if (ENV.NodeEnv === 'development') {
  app.use(morgan('dev'));
}

// Add correlationId to every request
app.use(correlationIdMiddleware);

// API Routes
app.use('/api/conversations', conversationRoute);
app.use('/api/conversations', messageRoute);
app.use(errorHandler);

// Static files
const viewsDir = path.join(__dirname, 'views');
const staticDir = path.join(__dirname, 'public');

app.set('views', viewsDir);
app.use(express.static(staticDir));

// Page Routes
app.get('/', (_: Request, res: Response) => res.redirect('/login'));
app.get('/login', (_: Request, res: Response) =>
  res.sendFile('login.html', { root: viewsDir }),
);
app.get('/chat', (_: Request, res: Response) =>
  res.sendFile('chat.html', { root: viewsDir }),
);

export default app;
