import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as UserController from '@/src/presentation/controllers/user.controller.js';
import { validateBody } from '@/src/presentation/middlewares/validation.middleware.js';
import { CreateUserRequestSchema } from '@/src/presentation/contracts/users/requests/create-user.request.js';

const router = Router();

router.get('/:id', asyncHandler(UserController.getUserByIdAsync));
router.post('/', validateBody(CreateUserRequestSchema), asyncHandler(UserController.createUserAsync));

export default router;