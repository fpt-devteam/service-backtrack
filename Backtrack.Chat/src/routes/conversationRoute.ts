import express from 'express';
import ConversationController from '../controllers/ConversationController';

/**
 * Conversation routes using class-based controller with @AsyncHandler decorator
 *
 * Notice: No need to wrap with asyncHandler() - the decorator handles it!
 */
const router = express.Router();

router.get(
  '/',
  ConversationController.getAllConversations.bind(ConversationController),
);
router.post(
  '/',
  ConversationController.createConversation.bind(ConversationController),
);
router.get(
  '/:id',
  ConversationController.getConversationById.bind(ConversationController),
);

export default router;
