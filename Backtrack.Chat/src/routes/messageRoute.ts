import express from 'express';
import MessageController from '../controllers/MessageController';

/**
 * Message routes using class-based controller with @AsyncHandler decorator
 *
 * Notice: No need to wrap with asyncHandler() - the decorator handles it!
 */
const router = express.Router();

router.post(
  '/:conversationId',
  MessageController.sendMessage.bind(MessageController),
);
router.get(
  '/:conversationId',
  MessageController.getMessages.bind(MessageController),
);

export default router;