import { Router } from 'express';
import * as conversationController from '@/controllers/conversation.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.post('/', asyncHandler(conversationController.createConversation));
router.get('/', asyncHandler(conversationController.listConversations));
router.get('/:id', asyncHandler(conversationController.getConversationById));
router.put('/:id', asyncHandler(conversationController.updateConversation));
router.delete('/:id', asyncHandler(conversationController.deleteConversation));
router.get('/queue/staff', asyncHandler(conversationController.listConversationQueueByStaff));
router.get('/assigned/staff', asyncHandler(conversationController.listConversationAssignedByStaff));
router.post('/:id/assign-staff', asyncHandler(conversationController.assignStaff));
router.post('/:id/unassign-staff', asyncHandler(conversationController.unassignStaff));

export default router;
