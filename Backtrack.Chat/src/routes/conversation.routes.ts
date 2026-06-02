import { Router } from 'express';
import * as conversationController from '@/controllers/conversation.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.post('/direct', asyncHandler(conversationController.createDirectConversation));
router.post('/organization', asyncHandler(conversationController.createOrgConversation));
router.get('/', asyncHandler(conversationController.listAllConversations));
router.get('/direct', asyncHandler(conversationController.listDirectConversations));
router.get('/partner', asyncHandler(conversationController.getConversationByPartnerId));
router.get('/organization/queue', asyncHandler(conversationController.listConversationQueueByStaff));
router.get('/organization/assigned', asyncHandler(conversationController.listConversationAssignedByStaff));
router.get('/organization/resolved', asyncHandler(conversationController.listConversationResolvedByStaff));
router.get('/organization/verified', asyncHandler(conversationController.listConversationVerifiedByStaff));
router.get('/organization/rejected', asyncHandler(conversationController.listConversationRejectedByStaff));
router.get('/organization/posts/:postId', asyncHandler(conversationController.listConversationsByPostId));
router.post('/organization/posts/:postId/close', asyncHandler(conversationController.closeConversationsByPostId));
router.get('/:id', asyncHandler(conversationController.getConversationById));
router.delete('/:id', asyncHandler(conversationController.deleteConversation));
router.post('/:id/assign-staff', asyncHandler(conversationController.assignStaff));
router.post('/:id/unassign-staff', asyncHandler(conversationController.unassignStaff));
router.post('/:id/verify', asyncHandler(conversationController.verifyConversation));
router.post('/:id/resolve', asyncHandler(conversationController.resolveConversation));
router.post('/:id/reject', asyncHandler(conversationController.rejectConversation));
router.post('/:id/support-form-data', asyncHandler(conversationController.updateSupportFormDataInConversation));

export default router;
