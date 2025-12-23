import { Router } from 'express';
import asyncHandler from "express-async-handler";

import * as itemController from '@/src/controllers/item.controller.js';

const router = Router();

router.get('/', asyncHandler(itemController.getAllAsync));
router.get('/:id', asyncHandler(itemController.getByIdAsync));
router.post('/', asyncHandler(itemController.createAsync));
export default router;
