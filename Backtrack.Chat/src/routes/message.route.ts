import express from 'express';
import MessageController from '@src/controllers/message.controller';

/**
 * Message routes
 *
 * GET    /api/chat/messages/:conversationId  - Get messages in a conversation (paginated)
 * POST   /api/chat/messages/:conversationId  - Send a message to a conversation
 */
const router = express.Router();

// Get messages in a conversation
// Query params: cursor (ISO date string), limit (number)
router.get(
  '/:conversationId',
  MessageController.getMessages.bind(MessageController),
);

// Send a message
// Body: { content }
router.post(
  '/:conversationId',
  MessageController.sendMessage.bind(MessageController),
);

export default router;