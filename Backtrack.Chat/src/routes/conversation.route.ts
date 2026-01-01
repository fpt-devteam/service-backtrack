import express from 'express';
import ConversationController from '@src/controllers/conversation.controller';

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

// Modify participant nickname in a conversation
// Body: { targetUserId, newNickname }
router.put(
  '/:id/nickname',
  ConversationController.modifyConversationParticipantNickname.bind(
    ConversationController),
);

export default router;
