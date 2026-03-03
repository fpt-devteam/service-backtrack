import { Router } from 'express';
import conversationRoutes from './conversation.routes';
import messageRoutes from './message.routes';

const router = Router();

// Health check
router.get('/health', (_req, res) =>
	res.status(200).json({
		status: 'OK',
		timestamp: new Date().toISOString(),
	}),
);

// All other route groups
router.use('/conversations', conversationRoutes);
router.use('/conversations/:conversationId/messages', messageRoutes);


export default router;
