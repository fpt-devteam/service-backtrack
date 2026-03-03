import { Router } from 'express';
import * as conversationController from '@/controllers/conversation.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.post('/', asyncHandler(conversationController.createConversation));
router.get('/', asyncHandler(conversationController.listConversations));
router.get('/:id', asyncHandler(conversationController.getConversationById));
router.put('/:id', asyncHandler(conversationController.updateConversation));
router.delete('/:id', asyncHandler(conversationController.deleteConversation));

export default router;
