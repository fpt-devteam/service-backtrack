import express from 'express';
import ConversationController from '@src/controllers/conversation.controller';

/**
 * Conversation routes
 *
 * GET    /api/chat/conversations      - Get all conversations for current user (paginated)
 * POST   /api/chat/conversations      - Create a new conversation
 * GET    /api/chat/conversations/:id  - Get conversation by ID
 */
const router = express.Router();

// Get all conversations (with pagination support)
// Query params: limit (number), cursor (ISO date string)
router.get(
  '/',
  ConversationController.getAllConversations.bind(ConversationController),
);

// Create new conversation
// Body: { partnerId, creatorKeyName?, partnerKeyName?, customAvatarUrl? }
router.post(
  '/',
  ConversationController.createConversation.bind(ConversationController),
);

// Get conversation by ID
router.get(
  '/:id',
  ConversationController.getConversationById.bind(ConversationController),
);

export default router;
