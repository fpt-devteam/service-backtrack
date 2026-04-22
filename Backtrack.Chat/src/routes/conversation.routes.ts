import { Router } from 'express';
import * as conversationController from '@/controllers/conversation.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.post('/direct', asyncHandler(conversationController.createDirectConversation));
router.post('/organization', asyncHandler(conversationController.createOrgConversation));
router.get('/', asyncHandler(conversationController.listAllConversations));
router.get('/direct', asyncHandler(conversationController.listDirectConversations));
router.get('/partner', asyncHandler(conversationController.getConversationByPartnerId));
router.get('/:id', asyncHandler(conversationController.getConversationById));
router.delete('/:id', asyncHandler(conversationController.deleteConversation));
router.get('/organization/queue', asyncHandler(conversationController.listConversationQueueByStaff));
router.get('/organization/assigned', asyncHandler(conversationController.listConversationAssignedByStaff));
router.post('/:id/assign-staff', asyncHandler(conversationController.assignStaff));
router.post('/:id/unassign-staff', asyncHandler(conversationController.unassignStaff));
router.post('/:id/resolve', asyncHandler(conversationController.resolveConversation));
router.get('/organization/resolved', asyncHandler(conversationController.listConversationResolvedByStaff));

export default router;
