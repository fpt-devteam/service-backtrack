import { Router } from 'express';
import * as userController from '@/controllers/user.controller';
import { asyncHandler } from '@/middlewares/async-handler';

const router = Router();

router.get('/', asyncHandler(userController.getAllUsers));
router.post('/', asyncHandler(userController.createUser));

export default router;
