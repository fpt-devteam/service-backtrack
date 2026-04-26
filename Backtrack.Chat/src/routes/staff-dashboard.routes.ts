import { Router } from 'express';
import * as staffDashboardController from '@/controllers/staff-dashboard.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.get('/chat-analys', asyncHandler(staffDashboardController.getStaffChatStats));

export default router;
