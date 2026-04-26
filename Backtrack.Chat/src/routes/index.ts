import { Router } from 'express';
import conversationRoutes from './conversation.routes';
import messageRoutes from './message.routes';
import staffDashboardRoutes from './staff-dashboard.routes';

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
router.use('/staff/dashboard', staffDashboardRoutes);


export default router;
