import { Router } from 'express';
import * as messageController from '@/controllers/message.controller';
import { asyncHandler } from '@/middlewares/async-handler';

// mergeParams: true to access :conversationId from parent router
const router = Router({ mergeParams: true });

router.get('/', asyncHandler(messageController.getMessagesByConversationId));

export default router;
